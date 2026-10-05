using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Data;
using TeamFlow.API.Services.Interfaces;
using TeamFlow.API.Models.Enums;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardResponseDto>
        GetDashboardAsync()
    {
        var organizationId = _currentUser.OrganizationId;
        var userId = _currentUser.UserId;
        var isMember = _currentUser.Role == "Member";

        var projectsQuery = _context.Projects
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        var tasksQuery = _context.Tasks
            .AsNoTracking()
            .Where(x => x.Project.OrganizationId == organizationId);

        // Members only see figures for the projects they belong to,
        // matching how the task and project endpoints behave.
        if (isMember)
        {
            projectsQuery = projectsQuery.Where(p =>
                p.ProjectMembers.Any(pm => pm.UserId == userId));

            tasksQuery = tasksQuery.Where(t =>
                t.Project.ProjectMembers.Any(pm => pm.UserId == userId));
        }

        var now = DateTime.UtcNow;

        var projectsCount = await projectsQuery.CountAsync();

        var membersCount = await _context.OrganizationMembers
            .CountAsync(x => x.OrganizationId == organizationId);

        var totalTasks = await tasksQuery.CountAsync();

        var overdueTasks = await tasksQuery
            .CountAsync(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value < now &&
                x.Status != TaskStatuss.Done);

        var grouped = await tasksQuery
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var statistics = new TaskStatisticsDto();

        foreach (var item in grouped)
        {
            switch (item.Status)
            {
                case TaskStatuss.Todo:
                    statistics.Todo = item.Count;
                    break;

                case TaskStatuss.InProgress:
                    statistics.InProgress = item.Count;
                    break;

                case TaskStatuss.Review:
                    statistics.Review = item.Count;
                    break;

                case TaskStatuss.Done:
                    statistics.Done = item.Count;
                    break;
            }
        }

        var recentTasks = await tasksQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(5)
            .Select(x => new RecentTaskDto
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                CreatedAt = x.CreatedAt,
                ProjectName = x.Project.Name
            })
            .ToListAsync();

        return new DashboardResponseDto
        {
            ProjectsCount = projectsCount,
            MembersCount = membersCount,
            TotalTasks = totalTasks,
            CompletedTasks = statistics.Done,
            InProgressTasks = statistics.InProgress,
            OverdueTasks = overdueTasks,

            TaskStatistics = statistics,
            RecentTasks = recentTasks
        };
    }
}
