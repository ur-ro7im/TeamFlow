using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.DTOs.Project;
using TeamFlow.API.DTOs.Projects;
using TeamFlow.API.Exceptions;
using TeamFlow.API.Interfaces;
using TeamFlow.API.Models;
using TeamFlow.API.Models.Enums;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Services;

public class ProjectService : IProjectService
{
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;
    private readonly IPlanLimitService _planLimitService;

    public ProjectService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IPlanLimitService planLimitService)
    {
        _context = context;
        _currentUser = currentUser;
        _planLimitService = planLimitService;
    }

    /// <summary>
    /// Projects the current user may see: everything in the organization for
    /// Owner/Admin, and only the projects they belong to for Members (the same
    /// rule TaskService already applies to tasks).
    /// </summary>
    private IQueryable<Project> VisibleProjects()
    {
        var organizationId = _currentUser.OrganizationId;

        var query = _context.Projects
            .Where(p => p.OrganizationId == organizationId);

        if (_currentUser.Role == "Member")
        {
            var userId = _currentUser.UserId;

            query = query.Where(p =>
                p.ProjectMembers.Any(pm => pm.UserId == userId));
        }

        return query;
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
    {
        return await VisibleProjects()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                OrganizationId = p.OrganizationId,
                TaskCount = p.Tasks.Count()
            })
            .ToListAsync();
    }

    public async Task<ProjectResponseDto?> GetByIdAsync(int id)
    {
        return await VisibleProjects()
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                OrganizationId = p.OrganizationId,
                TaskCount = p.Tasks.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto)
    {
        var organizationId = _currentUser.OrganizationId;

        if (dto.StartDate.HasValue &&
            dto.DueDate.HasValue &&
            dto.DueDate.Value < dto.StartDate.Value)
        {
            throw new BadRequestException(
                "Due date cannot be before the start date.");
        }

        var canCreate =
            await _planLimitService.CanCreateProjectAsync(organizationId);

        if (!canCreate)
        {
            throw new ConflictException(
                "Your current plan does not allow creating more projects.");
        }

        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim(),
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            Status = ProjectStatus.Planned,
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        return ToResponse(project, 0);
    }

    public async Task<ProjectResponseDto?> UpdateAsync(
        int id,
        UpdateProjectDto dto)
    {
        var organizationId = _currentUser.OrganizationId;

        var project = await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OrganizationId == organizationId);

        if (project is null)
            return null;

        project.Name = dto.Name.Trim();
        project.Description = string.IsNullOrWhiteSpace(dto.Description)
            ? null
            : dto.Description.Trim();

        await _context.SaveChangesAsync();

        var taskCount = await _context.Tasks
            .CountAsync(t => t.ProjectId == project.Id);

        return ToResponse(project, taskCount);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var organizationId = _currentUser.OrganizationId;

        var project = await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OrganizationId == organizationId);

        if (project is null)
            return false;

        _context.Projects.Remove(project);

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProjectResponseDto ToResponse(
        Project project,
        int taskCount)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            Status = project.Status.ToString(),
            CreatedAt = project.CreatedAt,
            OrganizationId = project.OrganizationId,
            TaskCount = taskCount
        };
    }
}
