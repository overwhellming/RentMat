using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Exceptions.Booking;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.IntegrationTests.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class GetBookingByIdHandlerTests : BaseIntegrationTest
{
    private readonly GetBookingByIdHandler _handler;
    private readonly IFusionCache _cache;
    
    public GetBookingByIdHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new  GetBookingByIdHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetBookingByIdHandler>>());
    }

    [Fact]
    public async Task Should_Return_Booking()
    {
        const decimal bookingPrice = 1000;
        var booking = await CreateBookingAsync(totalPrice: bookingPrice);
        var response = await _handler.Handle(booking.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(booking.Id);
        response.TotalPrice.Should().Be(bookingPrice);
    }

    [Fact]
    public async Task Should_Throw_BookingNotFoundException_When_BookingDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<BookingNotFoundException>(() =>
            _handler.Handle(notExistingId, CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_Return_CachedBooking_If_CacheExists()
    {
        const decimal initialPrice = 100;
        var booking = await CreateBookingAsync(totalPrice: initialPrice);
        
        var response = await _handler.Handle(booking.Id, CancellationToken.None);
        response.TotalPrice.Should().Be(initialPrice);

        var bookingInDb = await DbContext.Bookings.FindAsync(booking.Id);
        bookingInDb!.TotalPrice = initialPrice + 100;
        await DbContext.SaveChangesAsync();
        
        response = await _handler.Handle(booking.Id, CancellationToken.None);
        response.TotalPrice.Should().Be(initialPrice);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const decimal initialPrice = 100;
        var booking = await CreateBookingAsync(totalPrice: initialPrice);
        
        var response = await _handler.Handle(booking.Id, CancellationToken.None);
        response.TotalPrice.Should().Be(initialPrice);

        var bookingInDb = await DbContext.Bookings.FindAsync(booking.Id);
        bookingInDb!.TotalPrice = initialPrice + 100;
        await DbContext.SaveChangesAsync();
        await _cache.RemoveByTagAsync(CacheTags.Bookings);
        
        response = await _handler.Handle(booking.Id, CancellationToken.None);
        response.TotalPrice.Should().Be(initialPrice + 100);
    }
}