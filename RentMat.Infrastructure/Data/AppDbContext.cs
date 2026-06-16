using Microsoft.EntityFrameworkCore;
using RentMat.Core.Enums;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<DeviceCategory>().HasData(
            new DeviceCategory { Id = 1, Name = "Консоли" },
            new DeviceCategory { Id = 2, Name = "VR-Шлемы" },
            new DeviceCategory { Id = 3, Name = "Мощные ПК" }
        );

        modelBuilder.Entity<Device>().HasData(
            new Device
            {
                Id = 1,
                Name = "PlayStation 5 Pro",
                HourRentPrice = 150.00m,
                CategoryId = 1,
                Status = DeviceStatus.Rented
            },
            new Device
            {
                Id = 2,
                Name = "Xbox Series X",
                HourRentPrice = 120.00m,
                CategoryId = 1,
                Status = DeviceStatus.Rented
            },
            new Device
            {
                Id = 3,
                Name = "Meta Quest 3 (128GB)",
                HourRentPrice = 200.00m,
                CategoryId = 2,
                Status = DeviceStatus.Rented
            },
            new Device
            {
                Id = 4,
                Name = "ASUS ROG (RTX 4090, i9)",
                HourRentPrice = 350.00m,
                CategoryId = 3,
                Status = DeviceStatus.Maintenance
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Login = "Dovahkiin",
                Email = "dragonborn@tamriel.com",
                HashedPassword = "$2a$11$MX9xI7v09h4Lz6e6PZ7hCOgK8I.C2vS8iWx9wG1R1Wq8M2kM5G2t.",
                Balance = 5000.00m,
                CreatedAt = new DateTime(2007, 8, 29, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}