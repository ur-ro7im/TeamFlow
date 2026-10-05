using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
