using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class Project
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public ProjectStatus Status { get; set; } 

    public DateTime CreatedAt { get; set; }


    // Navigation Properties

    public Organization Organization { get; set; } = null!;

    public ICollection<ProjectMember> ProjectMembers
    {
        get;
        set;
    } = new List<ProjectMember>();

    public ICollection<TaskItem> Tasks
    {
        get;
        set;
    } = new List<TaskItem>();
}