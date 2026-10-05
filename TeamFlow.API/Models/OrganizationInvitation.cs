using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class OrganizationInvitation
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Email { get; set; } = null!;

    public OrganizationRole Role { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}