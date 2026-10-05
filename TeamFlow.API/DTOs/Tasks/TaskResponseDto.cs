namespace TeamFlow.API.DTOs.Tasks;

public class TaskResponseDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public DateTime? DueDate { get; set; }

    public int? AssignedToUserId { get; set; }

    public string? AssignedToUserName { get; set; }

    public int CreatedByUserId { get; set; }

    public string CreatedByUserName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int CommentsCount { get; set; }

    public int FilesCount { get; set; }
}