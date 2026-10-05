namespace TeamFlow.API.DTOs.Projects;

public class ProjectMemberResponseDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
}