using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class FileAttachmentConfiguration
    : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(x => x.Task)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedFiles)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TaskId);
    }
}