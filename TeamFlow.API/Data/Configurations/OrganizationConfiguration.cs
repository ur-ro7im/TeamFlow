using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class OrganizationConfiguration
    : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasMany(x => x.Projects)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Members)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subscription)
            .WithOne(x => x.Organization)
            .HasForeignKey<Subscription>(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Name);
    }
}