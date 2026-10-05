using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class OrganizationMemberConfiguration
    : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.Property(x => x.Role)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(x => x.User)
            .WithMany(x => x.OrganizationMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.OrganizationId
        })
        .IsUnique();
    }
}