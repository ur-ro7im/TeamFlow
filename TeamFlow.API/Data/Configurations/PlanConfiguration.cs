using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class PlanConfiguration
    : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(new Plan
        {
            Id = 1,
            Name = "Free",
            Price = 0,
            MaxMembers = 3,
            MaxProjects = 1,
            MaxStorage = 100L * 1024 * 1024
        },
         new Plan
         {
             Id = 2,
             Name = "Pro",
             Price = 19.99m,
             MaxMembers = 20,
             MaxProjects = 20,
             MaxStorage = 10L * 1024 * 1024 * 1024
         },

    new Plan
    {
        Id = 3,
        Name = "Business",
        Price = 49.99m,
        MaxMembers = 100,
        MaxProjects = 100,
        MaxStorage = 50L * 1024 * 1024 * 1024
    });
    }
}