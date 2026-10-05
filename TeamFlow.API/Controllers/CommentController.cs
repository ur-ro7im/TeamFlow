using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.DTOs.Comments;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(
        ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("task/{taskId:int}")]
    public async Task<IActionResult> GetTaskComments(
        int taskId)
    {
        var comments =
            await _commentService
                .GetTaskCommentsAsync(taskId);

        return Ok(comments);
    }

    [HttpPost("task/{taskId:int}")]
    public async Task<IActionResult> CreateComment(
        int taskId,
        CreateCommentDto dto)
    {
        var comment =
            await _commentService
                .CreateCommentAsync(
                    taskId,
                    dto);

        return CreatedAtAction(
            nameof(GetTaskComments),
            new
            {
                taskId
            },
            comment);
    }

    [HttpPut("{commentId:int}")]
    public async Task<IActionResult> UpdateComment(
        int commentId,
        CreateCommentDto dto)
    {
        await _commentService
            .UpdateCommentAsync(
                commentId,
                dto);

        return NoContent();
    }

    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> DeleteComment(
        int commentId)
    {
        await _commentService
            .DeleteCommentAsync(commentId);

        return NoContent();
    }
}