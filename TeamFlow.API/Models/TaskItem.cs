using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class TaskItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TaskStatuss Status { get; set; }

    public TaskPriority Priority { get; set; } 

    public DateTime? DueDate { get; set; }

    public int? AssignedToUserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeadlineReminderSentAt { get; set; }
    // Navigation Properties

    public Project Project { get; set; } = null!;

    public User CreatedByUser { get; set; } = null!;

    public User? AssignedToUser { get; set; }

    public ICollection<Comment> Comments
    {
        get;
        set;
    } = new List<Comment>();

    public ICollection<FileAttachment> Files
    {
        get;
        set;
    } = new List<FileAttachment>();
}