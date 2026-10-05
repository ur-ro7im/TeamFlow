public class DashboardResponseDto
{
    public int ProjectsCount { get; set; }

    public int MembersCount { get; set; }

    public int TotalTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int OverdueTasks { get; set; }

    public TaskStatisticsDto TaskStatistics { get; set; }
        = new();

    public List<RecentTaskDto> RecentTasks { get; set; }
    = new();
}