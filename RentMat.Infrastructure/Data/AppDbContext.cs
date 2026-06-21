using Microsoft.EntityFrameworkCore;
using RentMat.Core.Enums;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Deposit> Deposits => Set<Deposit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Login = "Dovahkiin",
                Email = "dragonborn@tamriel.com",
                HashedPassword = "hash",
                Balance = 5000m,
                CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Login = "Geralt",
                Email = "geralt@kaermorhen.com",
                HashedPassword = "hash",
                Balance = 15000m,
                CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 3,
                Login = "Shepard",
                Email = "shepard@normandy.com",
                HashedPassword = "hash",
                Balance = 25000m,
                CreatedAt = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc)
            }
        );

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
                HourRentPrice = 150m,
                CategoryId = 1,
                Status = DeviceStatus.Available
            },
            new Device
            {
                Id = 2,
                Name = "Xbox Series X",
                HourRentPrice = 120m,
                CategoryId = 1,
                Status = DeviceStatus.Rented
            },
            new Device
            {
                Id = 3,
                Name = "Meta Quest 3 (128GB)",
                HourRentPrice = 200m,
                CategoryId = 2,
                Status = DeviceStatus.Rented
            },
            new Device
            {
                Id = 4,
                Name = "ASUS ROG (RTX 4090, i9)",
                HourRentPrice = 350m,
                CategoryId = 3,
                Status = DeviceStatus.Maintenance
            }
        );

        modelBuilder.Entity<Booking>().HasData(
            new Booking
            {
                Id = 1,
                StartDate = new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 6, 10, 15, 0, 0, TimeSpan.Zero),
                TotalPrice = 750m,
                Status = BookingStatus.Completed,
                UserId = 1,
                DeviceId = 1
            },
            new Booking
            {
                Id = 2,
                StartDate = new DateTimeOffset(2026, 6, 20, 18, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 6, 22, 18, 0, 0, TimeSpan.Zero),
                TotalPrice = 5760m,
                Status = BookingStatus.Created,
                UserId = 2,
                DeviceId = 2
            },
            new Booking
            {
                Id = 3,
                StartDate = new DateTimeOffset(2026, 6, 16, 12, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 6, 17, 12, 0, 0, TimeSpan.Zero),
                TotalPrice = 4800m,
                Status = BookingStatus.Active,
                UserId = 3,
                DeviceId = 3
            },
            new Booking
            {
                Id = 4,
                StartDate = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 6, 25, 17, 0, 0, TimeSpan.Zero),
                TotalPrice = 960m,
                Status = BookingStatus.Cancelled,
                UserId = 1,
                DeviceId = 2
            }
        );
    }
}