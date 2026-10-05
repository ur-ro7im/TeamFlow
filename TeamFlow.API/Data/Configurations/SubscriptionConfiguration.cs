using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class SubscriptionConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(x => x.Status)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.Organization)
            .WithOne(x => x.Subscription)
            .HasForeignKey<Subscription>(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Subscription)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizationId)
            .IsUnique();
    }
}