using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Auth;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            throw new ConflictException(
                "Email is already registered."
            );
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            var organization = new Organization
            {
                Name = dto.OrganizationName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);

            // Every organization starts on the free plan. A paid plan chosen at sign-up is
            // only activated after a successful payment (POST /api/billing/change-plan),
            // so nobody gets paid limits without paying.
            var freePlan = await _context.Plans
                .OrderBy(x => x.Price)
                .FirstAsync();

            Plan? selectedPlan = null;

            if (dto.PlanId.HasValue)
            {
                selectedPlan = await _context.Plans
                    .FirstOrDefaultAsync(x => x.Id == dto.PlanId.Value);

                if (selectedPlan is null)
                {
                    throw new BadRequestException("The selected plan does not exist.");
                }
            }

            var subscription = new Subscription
            {
                Organization = organization,
                Plan = freePlan,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);


            var membership = new OrganizationMember
            {
                User = user,
                Organization = organization,
                Role = OrganizationRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _context.OrganizationMembers.Add(membership);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var token = _tokenService.CreateToken(user,organization.Id,membership.Role.ToString());

            return new AuthResponseDto
            {
                Token = token,

                UserId = user.Id,

                FullName = user.FullName,

                Email = user.Email,

                OrganizationId = organization.Id,

                Role = membership.Role.ToString(),

                SelectedPlanId = selectedPlan?.Id,

                RequiresPayment = selectedPlan is not null && selectedPlan.Price > 0
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<AuthResponseDto> LoginAsync(
    LoginDto dto)
    {
        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password."
            );
        }

        var passwordValid =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash
            );

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password."
            );
        }

        var membership =
            await _context.OrganizationMembers
                .FirstOrDefaultAsync(
                    x => x.UserId == user.Id
                );

        if (membership == null)
        {
            throw new ForbiddenException(
                "User is not associated with an organization."
            );
        }

        var token = _tokenService.CreateToken(
            user,
            membership.OrganizationId,
            membership.Role.ToString()
        );

        return new AuthResponseDto
        {
            Token = token,

            UserId = user.Id,

            FullName = user.FullName,

            Email = user.Email,

            OrganizationId = membership.OrganizationId,

            Role = membership.Role.ToString()
        };
    }

    public async Task<AuthResponseDto>
    RegisterFromInvitationAsync(
        RegisterFromInvitationDto dto)
    {
        var invitation =
            await _context.OrganizationInvitations
                .FirstOrDefaultAsync(x =>
                    x.Token == dto.Token);

        if (invitation == null)
        {
            throw new NotFoundException(
                "Invitation was not found.");
        }

        if (invitation.AcceptedAt.HasValue)
        {
            throw new ConflictException(
                "Invitation has already been accepted.");
        }

        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new ConflictException(
                "Invitation has expired.");
        }

        var email = invitation.Email;

        var existingUser =
            await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email);

        if (existingUser != null)
        {
            throw new ConflictException(
                "An account with this email already exists. " +
                "Please login and accept the invitation.");
        }

        var organization =
            await _context.Organizations
                .FirstOrDefaultAsync(x =>
                    x.Id == invitation.OrganizationId);

        if (organization == null)
        {
            throw new NotFoundException(
                "Organization was not found.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            var member = new OrganizationMember
            {
                User = user,
                OrganizationId = invitation.OrganizationId,
                Role = invitation.Role,
                JoinedAt = DateTime.UtcNow
            };

            _context.OrganizationMembers.Add(member);

            invitation.AcceptedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var token = _tokenService.CreateToken(
                user,
                invitation.OrganizationId,
                invitation.Role.ToString());

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                OrganizationId =
                    invitation.OrganizationId,
                Role = invitation.Role.ToString()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}