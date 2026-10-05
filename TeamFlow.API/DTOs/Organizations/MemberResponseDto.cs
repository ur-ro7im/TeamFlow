namespace TeamFlow.API.DTOs.Organizations;

public class MemberResponseDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
}