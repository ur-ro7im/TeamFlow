namespace TeamFlow.API.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }


    // Navigation Property

    public User User { get; set; } = null!;
}