using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Comments;

public class CreateCommentDto
{
    [Required]
    [MaxLength(3000)]
    public string Content { get; set; } = null!;
}
