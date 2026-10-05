using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class OrganizationInvitationConfiguration
    : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(
        EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Email
        });

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}