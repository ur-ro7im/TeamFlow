using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Services.Background;

/// <summary>
/// Paid plans last one month. There is no automatic renewal yet (the payment gateway is
/// simulated), so organizations whose paid period has ended move back to the Free plan and
/// the owners are notified. Nothing is deleted; Free limits only block creating new items.
/// </summary>
public class SubscriptionExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryService> _logger;

    public SubscriptionExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while expiring subscriptions.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ExpireSubscriptionsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context =
            scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var freePlan = await context.Plans
            .FirstOrDefaultAsync(p => p.Price <= 0, cancellationToken);

        if (freePlan is null)
        {
            return;
        }

        var expired = await context.Subscriptions
            .Include(s => s.Plan)
            .Where(s =>
                s.Status == SubscriptionStatus.Active &&
                s.EndDate.HasValue &&
                s.EndDate.Value < now &&
                s.Plan.Price > 0)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expired)
        {
            var previousPlanName = subscription.Plan.Name;

            subscription.PlanId = freePlan.Id;
            subscription.Plan = freePlan;
            subscription.StartDate = now;
            subscription.EndDate = null;

            var ownerIds = await context.OrganizationMembers
                .Where(m =>
                    m.OrganizationId == subscription.OrganizationId &&
                    m.Role == OrganizationRole.Owner)
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);

            foreach (var ownerId in ownerIds)
            {
                context.Notifications.Add(new Notification
                {
                    UserId = ownerId,
                    Message =
                        $"Your {previousPlanName} plan has ended and the organization is now on the {freePlan.Name} plan.",
                    IsRead = false,
                    CreatedAt = now
                });
            }
        }

        if (expired.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Moved {Count} expired subscription(s) to the Free plan.",
                expired.Count);
        }
    }
}
