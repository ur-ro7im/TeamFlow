using TeamFlow.API.DTOs.Comments;

namespace TeamFlow.API.Services.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponseDto>>
        GetTaskCommentsAsync(int taskId);

    Task<CommentResponseDto>
        CreateCommentAsync(
            int taskId,
            CreateCommentDto dto);

    Task UpdateCommentAsync(
        int commentId,
        CreateCommentDto dto);

    Task DeleteCommentAsync(
        int commentId);
}