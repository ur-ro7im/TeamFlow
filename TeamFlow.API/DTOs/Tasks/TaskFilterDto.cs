namespace TeamFlow.API.DTOs.Tasks;

public class TaskFilterDto
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public int? AssignedToUserId { get; set; }

    public DateTime? DueDateFrom { get; set; }

    public DateTime? DueDateTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}