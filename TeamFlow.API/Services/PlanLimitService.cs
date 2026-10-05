using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class PlanLimitService : IPlanLimitService
{
    private readonly AppDbContext _context;

    public PlanLimitService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanCreateProjectAsync(
        int organizationId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == SubscriptionStatus.Active);

        if (subscription is null)
            return false;

        var maxProjects = subscription.Plan.MaxProjects;

        // -1 means unlimited
        if (maxProjects == -1)
            return true;

        var currentProjects =
            await _context.Projects
                .CountAsync(p =>
                    p.OrganizationId == organizationId);

        return currentProjects < maxProjects;
    }

    public async Task<bool> CanAddMemberAsync(
        int organizationId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == SubscriptionStatus.Active);

        if (subscription is null)
            return false;

        var maxMembers = subscription.Plan.MaxMembers;

        // -1 means unlimited
        if (maxMembers == -1)
            return true;

        var currentMembers =
            await _context.OrganizationMembers
                .CountAsync(m =>
                    m.OrganizationId == organizationId);

        return currentMembers < maxMembers;
    }

    public async Task<bool> CanUseStorageAsync(
        int organizationId,
        long additionalBytes)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == SubscriptionStatus.Active);

        if (subscription is null)
            return false;

        var maxStorage =
            subscription.Plan.MaxStorage;

        // -1 means unlimited
        if (maxStorage == -1)
            return true;

        var usedStorage =
            await _context.Files
                .Where(f =>
                    f.Task.Project.OrganizationId == organizationId)
                .SumAsync(f => f.Size);

        return usedStorage + additionalBytes <= maxStorage;
    }
}