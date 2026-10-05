using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Services.Background;

public class DeadlineReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeadlineReminderService> _logger;

    public DeadlineReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<DeadlineReminderService> logger)
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
                await CheckDeadlinesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while checking deadlines.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(30),
                stoppingToken);
        }
    }

    private async Task CheckDeadlinesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var tomorrow = now.AddHours(24);

        var tasks = await context.Tasks
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value > now &&
                x.DueDate.Value <= tomorrow &&
                x.Status != TaskStatuss.Done &&
                x.AssignedToUserId.HasValue &&
                x.DeadlineReminderSentAt == null)
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            // Added to the same unit of work as the flag below, so a reminder
            // is never created twice (or flagged without being created).
            context.Notifications.Add(new Notification
            {
                UserId = task.AssignedToUserId!.Value,
                Message = $"The task '{task.Title}' is due within 24 hours.",
                IsRead = false,
                CreatedAt = now
            });

            task.DeadlineReminderSentAt = now;
        }

        await context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Deadline reminder check completed. " +
            "{Count} reminders created.",
            tasks.Count);
    }
}