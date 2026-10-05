namespace TeamFlow.API.Models;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }


    // Navigation Properties

    public ICollection<OrganizationMember> Members
    {
        get;
        set;
    } = new List<OrganizationMember>();

    public ICollection<Project> Projects
    {
        get;
        set;
    } = new List<Project>();

    public Subscription? Subscription { get; set; }

    public ICollection<Payment> Payments
    {
        get;
        set;
    } = new List<Payment>();
}