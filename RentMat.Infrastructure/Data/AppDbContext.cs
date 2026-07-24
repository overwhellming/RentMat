using Microsoft.EntityFrameworkCore;
using RentMat.Core.Enums;
using RentMat.Core.Models;

namespace RentMat.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceCategory> DeviceCategories => Set<DeviceCategory>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<RefreshTokenEntry> RefreshTokenEntries => Set<RefreshTokenEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}