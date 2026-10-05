using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Notifications;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Services.Interfaces;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task CreateAsync(
        int userId,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new BadRequestException(
                "Notification message is required.");
        }

        var notification = new Notification
        {
            UserId = userId,
            Message = message.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationResponseDto>>
      GetMyNotificationsAsync()
    {
        var userId = _currentUser.UserId;

        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new NotificationResponseDto
            {
                Id = x.Id,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = _currentUser.UserId;

        var notifications =
            await _context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(
       int notificationId)
    {
        var userId = _currentUser.UserId;

        var notification =
            await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

        if (notification == null)
        {
            throw new NotFoundException(
                "Notification not found.");
        }

        notification.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task<UnreadCountResponseDto>
    GetUnreadCountAsync()
    {
        var userId = _currentUser.UserId;

        var count = await _context.Notifications
            .CountAsync(x =>
                x.UserId == userId &&
                !x.IsRead);

        return new UnreadCountResponseDto
        {
            Count = count
        };
    }
}