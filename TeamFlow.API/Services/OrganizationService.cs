using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Organizations;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Helpers;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OrganizationService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task AddMemberAsync(
    AddMemberDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var email = dto.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        var existingMember =
     await _context.OrganizationMembers
         .FirstOrDefaultAsync(x =>
             x.OrganizationId == organizationId &&
             x.UserId == user.Id);

        if (existingMember != null)
            throw new ConflictException(
                "User is already a member of this organization.");

        var subscription =
            await _context.Subscriptions
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(
                    x => x.OrganizationId == organizationId);

        if (subscription == null)
        {
            throw new NotFoundException(
                "Organization subscription not found.");
        }

        if (subscription.Status !=
            SubscriptionStatus.Active)
        {
            throw new ForbiddenException(
                "Your subscription is not active.");
        }

        var membersCount =
            await _context.OrganizationMembers
                .CountAsync(x =>
                    x.OrganizationId == organizationId);

        if (subscription.Plan.MaxMembers != -1 &&
            membersCount >=
            subscription.Plan.MaxMembers)
        {
            throw new ConflictException(
                "Your plan has reached its member limit.");
        }

        if (!EnumParser.TryParseDefined<OrganizationRole>(
            dto.Role,
            true,
            out var role))
        {
            throw new BadRequestException(
                "Invalid organization role.");
        }

        if (role == OrganizationRole.Owner)
        {
            throw new BadRequestException(
                "You cannot add another Owner.");
        }

        var membership = new OrganizationMember
        {
            UserId = user.Id,
            OrganizationId = organizationId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(membership);

        await _context.SaveChangesAsync();
    }
    public async Task<List<MemberResponseDto>> GetMembersAsync()
    {
        var organizationId = _currentUser.OrganizationId;

        return await _context.OrganizationMembers
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Select(x => new MemberResponseDto
            {
                UserId = x.UserId,
                FullName = x.User.FullName,
                Email = x.User.Email,
                Role = x.Role.ToString(),
                JoinedAt = x.JoinedAt
            })
            .ToListAsync();
    }

    public async Task RemoveMemberAsync(int userId)
    {
        if (_currentUser.Role != "Owner")
        {
            throw new ForbiddenException(
                "Only the Owner can remove members.");
        }

        var organizationId =
            _currentUser.OrganizationId;

        var membership =
            await _context.OrganizationMembers
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId);

        if (membership == null)
        {
            throw new NotFoundException(
    "Member not found.");
        }

        if (membership.Role == OrganizationRole.Owner)
        {
            throw new BadRequestException(
                "The Owner cannot be removed.");
        }

        // Also drop the user's project memberships inside this organization.
        var projectMemberships =
            await _context.ProjectMembers
                .Where(x =>
                    x.UserId == userId &&
                    x.Project.OrganizationId == organizationId)
                .ToListAsync();

        _context.ProjectMembers.RemoveRange(projectMemberships);

        _context.OrganizationMembers.Remove(membership);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateMemberRoleAsync(
      int userId,
      UpdateMemberRoleDto dto)
    {
        if (_currentUser.Role != "Owner")
        {
            throw new ForbiddenException(
                "Only the Owner can change member roles.");
        }

        var organizationId =
            _currentUser.OrganizationId;

        var membership =
            await _context.OrganizationMembers
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId);

        if (membership == null)
        {
            throw new NotFoundException(
                "Member not found.");
        }

        if (membership.Role == OrganizationRole.Owner)
        {
            throw new BadRequestException(
                "The Owner's role cannot be changed.");
        }

        if (!EnumParser.TryParseDefined<OrganizationRole>(
            dto.Role,
            true,
            out var role))
        {
            throw new BadRequestException(
                "Invalid organization role.");
        }

        if (role == OrganizationRole.Owner)
        {
            throw new BadRequestException(
                "You cannot assign the Owner role.");
        }

        membership.Role = role;

        await _context.SaveChangesAsync();
    }

    // methods...

    public async Task<StorageResponseDto> GetStorageAsync()
    {
        var organizationId =
            _currentUser.OrganizationId;

        var subscription =
            await _context.Subscriptions
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.Status == SubscriptionStatus.Active);

        if (subscription == null)
        {
            throw new ConflictException(
                "Organization does not have an active subscription.");
        }

        var usedStorage =
            await _context.Files
                .Where(x =>
                    x.Task.Project.OrganizationId ==
                    organizationId)
                .SumAsync(x => x.Size);

        var maxStorage =
            subscription.Plan.MaxStorage;

        var percentage =
            maxStorage <= 0
                ? 0
                : usedStorage * 100.0 / maxStorage;

        return new StorageResponseDto
        {
            UsedStorage = usedStorage,
            MaxStorage = maxStorage,
            UsedPercentage = percentage
        };
    }
}