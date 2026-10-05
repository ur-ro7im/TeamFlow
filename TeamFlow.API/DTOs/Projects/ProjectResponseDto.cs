
namespace TeamFlow.API.DTOs.Projects;

public class ProjectResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int OrganizationId { get; set; }

    public int TaskCount { get; set; }
}