namespace TeamFlow.API.DTOs.Comments;

public class CommentResponseDto
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}