using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Projects;

public class CreateProjectDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }
}