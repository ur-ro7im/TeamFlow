using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Tasks;

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(5000)]
    public string? Description { get; set; }

    public string Priority { get; set; } = null!;

    public DateTime? DueDate { get; set; }

    public int? AssignedToUserId { get; set; }
}