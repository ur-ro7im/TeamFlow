using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Organizations;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Helpers;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class InvitationService : IInvitationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;

    public InvitationService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _context = context;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    public async Task<InvitationResponseDto>
        CreateInvitationAsync(InviteMemberDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                "Email is required.");
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
            throw new ConflictException(
                "Owner role cannot be assigned through invitation.");
        }

        var organization =
            await _context.Organizations
                .FirstOrDefaultAsync(x =>
                    x.Id == organizationId);

        if (organization == null)
        {
            throw new NotFoundException(
                "Organization was not found.");
        }

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

        var membersCount =
            await _context.OrganizationMembers
                .CountAsync(x =>
                    x.OrganizationId == organizationId);

        var pendingInvitationsCount =
            await _context.OrganizationInvitations
                .CountAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.AcceptedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow);

        if (subscription.Plan.MaxMembers != -1 &&
            membersCount + pendingInvitationsCount >=
            subscription.Plan.MaxMembers)
        {
            throw new ConflictException(
                "Member limit has been reached.");
        }

        var user =
            await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email);

        if (user != null)
        {
            var existingMember =
                await _context.OrganizationMembers
                    .AnyAsync(x =>
                        x.OrganizationId == organizationId &&
                        x.UserId == user.Id);

            if (existingMember)
            {
                throw new ConflictException(
                    "User is already a member of this organization.");
            }
        }

        var existingInvitation =
            await _context.OrganizationInvitations
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.Email == email &&
                    x.AcceptedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow);

        if (existingInvitation != null)
        {
            throw new ConflictException(
                "An active invitation already exists for this email.");
        }

        var tokenBytes =
            RandomNumberGenerator.GetBytes(32);

        var token =
            Microsoft.AspNetCore.WebUtilities.WebEncoders
                .Base64UrlEncode(tokenBytes);

        var invitation = new OrganizationInvitation
        {
            OrganizationId = organizationId,
            Email = email,
            Role = role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow
        };

        _context.OrganizationInvitations.Add(invitation);

        await _context.SaveChangesAsync();

        await _emailService.SendInvitationEmailAsync(
            email,
            organization.Name,
            token);

        return new InvitationResponseDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            ExpiresAt = invitation.ExpiresAt
        };
    }

    public async Task AcceptInvitationAsync(
        string token)
    {
        var userId = _currentUser.UserId;

        var invitation =
            await _context.OrganizationInvitations
                .FirstOrDefaultAsync(x =>
                    x.Token == token);

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

        var user =
            await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == userId);

        if (user == null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        if (!user.Email.Equals(
                invitation.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "Invitation email does not match the current user.");
        }

        var existingMember =
            await _context.OrganizationMembers
                .AnyAsync(x =>
                    x.OrganizationId ==
                        invitation.OrganizationId &&
                    x.UserId == userId);

        if (existingMember)
        {
            throw new ConflictException(
                "User is already a member of this organization.");
        }

        var member = new OrganizationMember
        {
            UserId = userId,
            OrganizationId = invitation.OrganizationId,
            Role = invitation.Role,
            JoinedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(member);

        invitation.AcceptedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}