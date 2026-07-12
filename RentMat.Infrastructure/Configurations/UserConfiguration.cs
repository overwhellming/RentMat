using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMat.Core.Constants;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(u => u.Login)
            .HasMaxLength(ValidationConstants.UserLoginMaxLength);
        b.Property(u => u.Email)
            .HasMaxLength(ValidationConstants.UserEmailMaxLength);
        b.Property(u => u.HashedPassword)
            .HasMaxLength(ValidationConstants.UserPasswordHashMaxLength);
        b.Property(u => u.Balance)
            .HasPrecision(18, 2);
        b.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(ValidationConstants.UserRoleMaxLength);
        
        b.HasIndex(u => u.Login)
            .IsUnique();
        b.HasIndex(u => u.Email)
            .IsUnique();
    }
}