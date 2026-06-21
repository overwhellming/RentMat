using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(u => u.Login)
            .HasMaxLength(50);
        b.Property(u => u.Email)
            .HasMaxLength(50);
        b.Property(u => u.HashedPassword)
            .HasMaxLength(100);
        b.Property(u => u.Balance)
            .HasPrecision(18, 2);
        b.Property(u => u.Role)
            .HasConversion<string>();
        
        b.HasIndex(u => u.Login)
            .IsUnique();
        b.HasIndex(u => u.Email)
            .IsUnique();
    }
}