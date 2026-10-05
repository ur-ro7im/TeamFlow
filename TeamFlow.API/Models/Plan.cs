namespace TeamFlow.API.Models;

public class Plan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int MaxMembers { get; set; }

    public int MaxProjects { get; set; }

    public long MaxStorage { get; set; }


    // Navigation Property

    public ICollection<Subscription> Subscriptions
    {
        get;
        set;
    } = new List<Subscription>();
}