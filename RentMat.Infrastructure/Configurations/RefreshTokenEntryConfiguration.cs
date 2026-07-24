using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class RefreshTokenEntryConfiguration : IEntityTypeConfiguration<RefreshTokenEntry>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntry> b)
    {
        b.Property(e => e.Token)
            .HasMaxLength(256);
        
        b.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}