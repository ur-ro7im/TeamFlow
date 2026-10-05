using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Comments;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Models;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CommentService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CommentResponseDto>
    CreateCommentAsync(
        int taskId,
        CreateCommentDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(
                dto.Content))
        {
            throw new BadRequestException(
                "Comment content is required.");
        }

        var task = await _context.Tasks
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                x.Project.OrganizationId ==
                    organizationId);

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        if (_currentUser.Role == "Member")
        {
            var isProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId ==
                            task.ProjectId &&
                        x.UserId == userId);

            if (!isProjectMember)
            {
                throw new ForbiddenException(
                    "You are not a member of this project.");
            }
        }

        var comment = new Comment
        {
            TaskId = taskId,
            UserId = userId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        // The people who own the task (assignee and creator) hear about the comment.
        // Added in the same SaveChanges so a comment never exists without its notifications.
        var recipients = new HashSet<int>();

        if (task.AssignedToUserId.HasValue)
        {
            recipients.Add(task.AssignedToUserId.Value);
        }

        recipients.Add(task.CreatedByUserId);
        recipients.Remove(userId);

        if (recipients.Count > 0)
        {
            var authorName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FullName)
                .FirstAsync();

            foreach (var recipientId in recipients)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = recipientId,
                    Message = $"{authorName} commented on the task '{task.Title}'.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        var result =
    await _context.Comments
        .AsNoTracking()
        .Where(x => x.Id == comment.Id)
        .Select(x => new CommentResponseDto
        {
            Id = x.Id,
            TaskId = x.TaskId,
            UserId = x.UserId,
            UserName = x.User.FullName,
            Content = x.Content,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        })
        .FirstAsync();

        return result;
    }

    public async Task<List<CommentResponseDto>>
    GetTaskCommentsAsync(int taskId)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                x.Project.OrganizationId ==
                    organizationId);

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        if (_currentUser.Role == "Member")
        {
            var isProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId ==
                            task.ProjectId &&
                        x.UserId == userId);

            if (!isProjectMember)
            {
                throw new NotFoundException(
                    "Task not found.");
            }
        }

        return await _context.Comments
            .AsNoTracking()
            .Where(x =>
                x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentResponseDto
            {
                Id = x.Id,
                TaskId = x.TaskId,
                UserId = x.UserId,
                UserName = x.User.FullName,
                Content = x.Content,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task DeleteCommentAsync(
    int commentId)
    {
        var comment = await _context.Comments
            .Include(x => x.Task)
                .ThenInclude(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Id == commentId &&
                x.Task.Project.OrganizationId ==
                    _currentUser.OrganizationId);

        if (comment == null)
        {
            throw new NotFoundException(
                "Comment not found.");
        }

        var isOwner =
            comment.UserId ==
            _currentUser.UserId;

        var isAdmin =
            _currentUser.Role == "Admin";

        var isOwnerRole =
            _currentUser.Role == "Owner";

        if (!isOwner &&
            !isAdmin &&
            !isOwnerRole)
        {
            throw new ForbiddenException(
                "You are not allowed to delete this comment.");
        }

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateCommentAsync(int commentId, CreateCommentDto dto)
    {
        var comment = await _context.Comments.Include(x => x.Task)
        .ThenInclude(x => x.Project)
    .FirstOrDefaultAsync(x =>
        x.Id == commentId &&
        x.Task.Project.OrganizationId ==
            _currentUser.OrganizationId);

        if (comment == null)
        {
            throw new NotFoundException(
                "Comment not found.");
        }
        var isOwner =
    comment.UserId ==
    _currentUser.UserId;

        var isAdmin =
            _currentUser.Role == "Admin";

        var isOwnerRole =
            _currentUser.Role == "Owner";

        if (!isOwner && !isAdmin && !isOwnerRole)
        {
            throw new ForbiddenException(
                "You are not allowed to edit this comment.");
        }

        if (string.IsNullOrWhiteSpace(
        dto.Content))
        {
            throw new BadRequestException(
                "Comment content is required.");
        }

        comment.Content =
            dto.Content.Trim();

        comment.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}