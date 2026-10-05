using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Tasks;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Helpers;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    public TaskService(
    AppDbContext context,
    ICurrentUserService currentUser,
    INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<TaskResponseDto> CreateTaskAsync(
      int projectId,
      CreateTaskDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        var project = await _context.Projects
            .FirstOrDefaultAsync(x =>
                x.Id == projectId &&
                x.OrganizationId == organizationId);

        if (project == null)
        {
            throw new NotFoundException(
                "Project not found.");
        }

        if (_currentUser.Role == "Member")
        {
            var isProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId == projectId &&
                        x.UserId == userId);

            if (!isProjectMember)
            {
                throw new ForbiddenException(
                    "You are not a member of this project.");
            }
        }

        if (dto.AssignedToUserId.HasValue)
        {
            var isProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId == projectId &&
                        x.UserId ==
                            dto.AssignedToUserId.Value);

            if (!isProjectMember)
            {
                throw new BadRequestException(
                    "Assigned user is not a member of this project.");
            }
        }

        if (!EnumParser.TryParseDefined<TaskPriority>(
                dto.Priority,
                true,
                out var priority))
        {
            throw new BadRequestException(
                "Invalid task priority.");
        }

        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = priority,
            Status = TaskStatuss.Todo,
            DueDate = dto.DueDate,
            AssignedToUserId =
                dto.AssignedToUserId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        if (dto.AssignedToUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                dto.AssignedToUserId.Value,
                $"You have been assigned the task '{task.Title}'.");
        }

        return await GetTaskByIdAsync(task.Id);
    }

    public async Task DeleteTaskAsync(
    int taskId)
    {
        var organizationId =
            _currentUser.OrganizationId;

        if (_currentUser.Role == "Member")
        {
            throw new ForbiddenException(
                "Members cannot delete tasks.");
        }

        var task = await _context.Tasks
            .FirstOrDefaultAsync(x =>
                x.Id == taskId &&
                x.Project.OrganizationId ==
                    organizationId);

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();
    }

    public async Task<PagedResultDto<TaskResponseDto>>
     GetProjectTasksAsync(
         int projectId,
         TaskFilterDto filter)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        var query = _context.Tasks
            .AsNoTracking()
            .Where(x =>
                x.ProjectId == projectId &&
                x.Project.OrganizationId ==
                    organizationId);

        if (_currentUser.Role == "Member")
        {
            query = query.Where(x =>
                x.Project.ProjectMembers.Any(pm =>
                    pm.UserId == userId));
        }

        if (!string.IsNullOrWhiteSpace(
                filter.Search))
        {
            var search =
                filter.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(
                filter.Status))
        {
            if (!EnumParser.TryParseDefined<TaskStatuss>(
                    filter.Status,
                    true,
                    out var status))
            {
                throw new BadRequestException(
                    "Invalid task status.");
            }

            query = query.Where(x =>
                x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(
                filter.Priority))
        {
            if (!EnumParser.TryParseDefined<TaskPriority>(
                    filter.Priority,
                    true,
                    out var priority))
            {
                throw new BadRequestException(
                    "Invalid task priority.");
            }

            query = query.Where(x =>
                x.Priority == priority);
        }

        if (filter.AssignedToUserId.HasValue)
        {
            query = query.Where(x =>
                x.AssignedToUserId ==
                    filter.AssignedToUserId.Value);
        }

        if (filter.DueDateFrom.HasValue)
        {
            query = query.Where(x =>
                x.DueDate >=
                    filter.DueDateFrom.Value);
        }

        if (filter.DueDateTo.HasValue)
        {
            query = query.Where(x =>
                x.DueDate <=
                    filter.DueDateTo.Value);
        }

        var totalCount =
            await query.CountAsync();

        if (filter.PageNumber < 1)
        {
            filter.PageNumber = 1;
        }

        if (filter.PageSize < 1)
        {
            filter.PageSize = 10;
        }

        if (filter.PageSize > 100)
        {
            filter.PageSize = 100;
        }

        var skip =
            (filter.PageNumber - 1) *
            filter.PageSize;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(x => new TaskResponseDto
            {
                Id = x.Id,

                ProjectId = x.ProjectId,

                Title = x.Title,

                Description = x.Description,

                Status = x.Status.ToString(),

                Priority = x.Priority.ToString(),

                DueDate = x.DueDate,

                AssignedToUserId =
                    x.AssignedToUserId,

                AssignedToUserName =
                    x.AssignedToUser != null
                        ? x.AssignedToUser.FullName
                        : null,

                CreatedByUserId =
                    x.CreatedByUserId,

                CreatedByUserName =
                    x.CreatedByUser.FullName,

                CreatedAt = x.CreatedAt,

                UpdatedAt = x.UpdatedAt,

                CommentsCount = x.Comments.Count(),

                FilesCount = x.Files.Count()
            })
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)filter.PageSize);

        return new PagedResultDto<TaskResponseDto>
        {
            Items = items,

            PageNumber =
                filter.PageNumber,

            PageSize =
                filter.PageSize,

            TotalCount =
                totalCount,

            TotalPages =
                totalPages
        };
    }

    public async Task<TaskResponseDto> GetTaskByIdAsync(
      int taskId)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        var query = _context.Tasks
            .AsNoTracking()
            .Where(x =>
                x.Id == taskId &&
                x.Project.OrganizationId ==
                    organizationId);

        if (_currentUser.Role == "Member")
        {
            query = query.Where(x =>
                x.Project.ProjectMembers.Any(pm =>
                    pm.UserId == userId));
        }

        var task = await query
            .Select(x => new TaskResponseDto
            {
                Id = x.Id,

                ProjectId = x.ProjectId,

                Title = x.Title,

                Description = x.Description,

                Status = x.Status.ToString(),

                Priority = x.Priority.ToString(),

                DueDate = x.DueDate,

                AssignedToUserId =
                    x.AssignedToUserId,

                AssignedToUserName =
                    x.AssignedToUser != null
                        ? x.AssignedToUser.FullName
                        : null,

                CreatedByUserId =
                    x.CreatedByUserId,

                CreatedByUserName =
                    x.CreatedByUser.FullName,

                CreatedAt = x.CreatedAt,

                UpdatedAt = x.UpdatedAt,

                CommentsCount = x.Comments.Count(),

                FilesCount = x.Files.Count()
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            throw new NotFoundException(
                "Task not found.");
        }

        return task;
    }

    public async Task UpdateTaskAsync(
      int taskId,
      UpdateTaskDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

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

        if (dto.AssignedToUserId.HasValue)
        {
            var isProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId ==
                            task.ProjectId &&
                        x.UserId ==
                            dto.AssignedToUserId.Value);

            if (!isProjectMember)
            {
                throw new BadRequestException(
                    "Assigned user is not a member of this project.");
            }
        }

        if (!EnumParser.TryParseDefined<TaskPriority>(
                dto.Priority,
                true,
                out var priority))
        {
            throw new BadRequestException(
                "Invalid task priority.");
        }

        task.Title =
            dto.Title.Trim();

        task.Description =
            dto.Description?.Trim();

        task.Priority =
            priority;

        task.DueDate =
            dto.DueDate;

        var previousAssigneeId = task.AssignedToUserId;

        task.AssignedToUserId =
            dto.AssignedToUserId;

        task.UpdatedAt =
            DateTime.UtcNow;

        // Tell the new assignee (not the person who made the change).
        if (dto.AssignedToUserId.HasValue &&
            dto.AssignedToUserId != previousAssigneeId &&
            dto.AssignedToUserId.Value != userId)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = dto.AssignedToUserId.Value,
                Message = $"You have been assigned the task '{task.Title}'.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskStatusAsync(
        int taskId,
        UpdateTaskStatusDto dto)
    {
        var organizationId =
            _currentUser.OrganizationId;

        var userId =
            _currentUser.UserId;

        var task = await _context.Tasks
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

        if (!EnumParser.TryParseDefined<TaskStatuss>(
                dto.Status,
                true,
                out var status))
        {
            throw new BadRequestException(
                "Invalid task status.");
        }

        task.Status = status;

        task.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }


}