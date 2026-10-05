using TeamFlow.API.DTOs.Notifications;
namespace TeamFlow.API.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(
        int userId,
        string message);

    Task<List<NotificationResponseDto>>
        GetMyNotificationsAsync();

    Task MarkAsReadAsync(int notificationId);

    Task MarkAllAsReadAsync();
    Task<UnreadCountResponseDto> GetUnreadCountAsync();
}