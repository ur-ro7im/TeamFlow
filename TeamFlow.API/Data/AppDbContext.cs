using Microsoft.EntityFrameworkCore;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<OrganizationMember> OrganizationMembers { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectMember> ProjectMembers { get; set; }

    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<FileAttachment> Files { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Plan> Plans { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<OrganizationInvitation>
    OrganizationInvitations
    { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

  
}

