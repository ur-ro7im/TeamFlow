using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class TaskItemConfiguration
    : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(5000);

        builder.Property(x => x.Status)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Priority)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedToUser)
            .WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProjectId);

        builder.HasIndex(x => x.AssignedToUserId);
    }
}