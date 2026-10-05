namespace TeamFlow.API.Models;

public class Comment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    // Navigation Properties

    public TaskItem Task { get; set; } = null!;

    public User User { get; set; } = null!;
}