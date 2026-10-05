using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var notifications =
            await _notificationService
                .GetMyNotificationsAsync();

        return Ok(notifications);
    }

    [HttpPatch("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(
        int notificationId)
    {
        await _notificationService
            .MarkAsReadAsync(notificationId);

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService
            .MarkAllAsReadAsync();

        return NoContent();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result =
            await _notificationService.GetUnreadCountAsync();

        return Ok(result);
    }
}