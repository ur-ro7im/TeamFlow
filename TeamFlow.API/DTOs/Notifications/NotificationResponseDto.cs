namespace TeamFlow.API.DTOs.Notifications;

public class NotificationResponseDto
{
    public int Id { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}