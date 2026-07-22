using FluentAssertions;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.IntegrationTests.Infrastructure;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class GetUserBookingsHandlerTests : BaseIntegrationTest
{
    private readonly GetUserBookingsHandler _handler;
    
    public GetUserBookingsHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _handler = new GetUserBookingsHandler(DbContext);
    }

    [Fact]
    public async Task Should_Return_UserBookings()
    {
        const int bookingCount = 3;
        var bookings = await CreateBookingsAsync(bookingCount);
        var response = await _handler.Handle(bookings.UserId, CancellationToken.None);
        response.Should().HaveCount(bookingCount);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_WhenUserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(notExistingId, CancellationToken.None));
    }
}