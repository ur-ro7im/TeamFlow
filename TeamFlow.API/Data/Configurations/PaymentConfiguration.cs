using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.API.Models;

namespace TeamFlow.API.Data.Configurations;

public class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .IsRequired().HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.TransactionId)
            .HasMaxLength(200);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Subscription)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TransactionId);
    }
}