using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class OrganizationMember
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int OrganizationId { get; set; }

    public OrganizationRole Role { get; set; } 

    public DateTime JoinedAt { get; set; }


    // Navigation Properties

    public User User { get; set; } = null!;

    public Organization Organization { get; set; } = null!;
}