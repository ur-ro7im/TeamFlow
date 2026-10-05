using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}