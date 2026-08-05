using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Commands.Booking;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Booking;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class CompleteBookingCommandHandlerTests : BaseIntegrationTest
{
    private readonly CompleteBookingCommandHandler _commandHandler;
    private readonly IFusionCache _cache;
    
    public CompleteBookingCommandHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _commandHandler = scope.ServiceProvider.GetRequiredService<CompleteBookingCommandHandler>();
    }

    [Fact]
    public async Task Should_CompleteBooking_And_SetDeviceAvailable_AndSetEndDate_In_Database()
    {
        var booking = await CreateBookingAsync();
        booking.Status.Should().Be(BookingStatus.Active);
        
        await _commandHandler.Handle(new CompleteBookingCommand(booking.Id, booking.UserId), CancellationToken.None);

        DbContext.ChangeTracker.Clear();
        
        var bookingInDB = await DbContext.Bookings
            .Include(b => b.Device)
            .FirstOrDefaultAsync(b => b.Id == booking.Id);
        
        bookingInDB!.Status.Should().Be(BookingStatus.Completed);
        bookingInDB.Device.Status.Should().Be(DeviceStatus.Available);
        bookingInDB.EndDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Should_Throw_ActiveBookingNotFoundException_When_Device_Is_Not_Booked()
    {
        var booking = await CreateBookingAsync(status: BookingStatus.Completed);

        await Assert.ThrowsAsync<ActiveBookingNotFoundException>(() =>
            _commandHandler.Handle(new CompleteBookingCommand(booking.Id, booking.UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_Throw_BookingAccessDeniedException_When_UserId_Is_Different()
    {
        const int otherUserId = 999;
        var booking = await CreateBookingAsync();

        await Assert.ThrowsAsync<BookingAccessDeniedException>(() =>
            _commandHandler.Handle(new CompleteBookingCommand(booking.Id, otherUserId), CancellationToken.None));
    }

    [Fact]
    public async Task Should_InvalidateCache_After_Complete()
    {
        var booking = await CreateBookingAsync();

        const string cacheKey = "key";
        await _cache.SetAsync(cacheKey, "test", tags: [CacheTags.Bookings]);
        var cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeTrue();
        
        await _commandHandler.Handle(new CompleteBookingCommand(booking.Id, booking.UserId), CancellationToken.None);
        
        cachedData = await _cache.TryGetAsync<string>(cacheKey);
        cachedData.HasValue.Should().BeFalse();
    }
}