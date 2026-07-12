using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class DepositConfiguration : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> b)
    {
        b.Property(d => d.Amount)
            .HasPrecision(18, 2);

        b.HasOne(d => d.User)
            .WithMany(u => u.Deposits)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasIndex(d => d.UserId);
        b.HasIndex(d => new { d.UserId, d.CreatedAt });
    }
}