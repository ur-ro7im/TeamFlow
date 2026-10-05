public class RecentTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string ProjectName { get; set; } = null!;
}