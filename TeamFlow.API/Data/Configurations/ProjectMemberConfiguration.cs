using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class ProjectMemberConfiguration
    : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(
        EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.ProjectId,
            x.UserId
        })
        .IsUnique();

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectMembers)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ProjectMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}