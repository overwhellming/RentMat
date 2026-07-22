using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Common;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class CreateBookingHandlerTests : BaseIntegrationTest
{
    private readonly CreateBookingHandler _handler;
    private readonly IFusionCache _cache;
    
    public CreateBookingHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new CreateBookingHandler(DbContext, _cache);
    }

    [Fact]
    public async Task Should_CreateBooking_In_Database()
    {
        const decimal initialBalance = 1000m;
        const decimal hourRentPrice = 20m;
        
        var user = await CreateUserAsync(balance: initialBalance);
        var device = await CreateDeviceAsync(hourRentPrice: hourRentPrice);

        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        var dto = new BookingCreateDto(device.Id,
            user.Id,
            startDate, 
            endDate);
        
        var response = await _handler.Handle(dto, CancellationToken.None);
        response.Should().NotBeNull();
        response.DeviceName.Should().Be(device.Name);
        response.Login.Should().Be(user.Login);
        response.StartDate.Should().Be(startDate);
        response.EndDate.Should().Be(endDate);
        response.StatusName.Should().Be(nameof(BookingStatus.Created));

        var bookingInDb = await DbContext.Bookings.FindAsync(response.Id);
        bookingInDb.Should().NotBeNull();
        bookingInDb.DeviceId.Should().Be(device.Id);
        bookingInDb.UserId.Should().Be(user.Id);
        bookingInDb.StartDate.Should().Be(startDate);
        bookingInDb.EndDate.Should().Be(endDate);
        bookingInDb.Status.Should().Be(BookingStatus.Created);

        var userInDb = await DbContext.Users.FindAsync(user.Id);
        
        var hours = (decimal)(dto.EndDate - dto.StartDate).TotalMinutes / 60m;
        var totalPrice = hours * hourRentPrice;

        userInDb!.Balance.Should().Be(initialBalance - totalPrice);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_When_UserDoesNotExist()
    {
        const int notExistingId = 999;
        var device = await CreateDeviceAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        var dto = new BookingCreateDto(device.Id,
            notExistingId,
            startDate, 
            endDate);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(dto, CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_DeviceNotFoundException_When_Device_DoesNotExist()
    {
        const int notExistingId = 999;
        var user = await CreateUserAsync();
        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        var dto = new BookingCreateDto(notExistingId,
            user.Id,
            startDate, 
            endDate);
        
        await Assert.ThrowsAsync<DeviceNotFoundException>(() => _handler.Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_DeviceIsNotAvailableException_When_Device_IsNot_Available()
    {
        const decimal initialBalance = 1000m;
        const decimal hourRentPrice = 20m;
        
        var user = await CreateUserAsync(balance: initialBalance);
        var device = await CreateDeviceAsync(hourRentPrice: hourRentPrice, status: DeviceStatus.Maintenance);

        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        var dto = new BookingCreateDto(device.Id,
            user.Id,
            startDate, 
            endDate);
        
        await Assert.ThrowsAsync<DeviceIsNotAvailableException>(() => _handler.Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_DeviceIsBookedException_When_Device_Is_Booked()
    {
        const decimal initialBalance = 1000m;
        const decimal hourRentPrice = 20m;
        
        var user = await CreateUserAsync(balance: initialBalance);
        var device = await CreateDeviceAsync(hourRentPrice: hourRentPrice);
        
        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        await CreateBookingAsync(startDate: startDate, endDate: endDate, deviceId: device.Id, userId: user.Id);
        
        var dto = new BookingCreateDto(device.Id,
            user.Id,
            startDate, 
            endDate);
        
        await Assert.ThrowsAsync<DeviceIsBookedException>(() => _handler.Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Throw_NotEnoughMoneyException_When_UserDoesNotHaveEnoughMoney()
    {
        const decimal initialBalance = 0;
        const decimal hourRentPrice = 20m;
        
        var user = await CreateUserAsync(balance: initialBalance);
        var device = await CreateDeviceAsync(hourRentPrice: hourRentPrice);
        
        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        
        var dto = new BookingCreateDto(device.Id,
            user.Id,
            startDate, 
            endDate);
        
        await Assert.ThrowsAsync<NotEnoughMoneyException>(() => _handler.Handle(dto, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_InvalidateCache_After_Creation()
    {
        const decimal initialBalance = 1000;
        const decimal hourRentPrice = 20m;
        
        var user = await CreateUserAsync(balance: initialBalance);
        var device = await CreateDeviceAsync(hourRentPrice: hourRentPrice);
        
        var startDate = DateTimeOffset.UtcNow.AddDays(1);
        var endDate = DateTimeOffset.UtcNow.AddDays(2);
        var dto = new BookingCreateDto(device.Id,
            user.Id,
            startDate, 
            endDate);

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Bookings]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();
        
        await _handler.Handle(dto, CancellationToken.None);
        
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}