using FluentAssertions;
using RentMat.Application.Exceptions.Users;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries.Booking;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class GetUserBookingsQueryHandlerTests : BaseIntegrationTest
{
    private readonly GetUserBookingsQueryHandler _queryHandler;
    
    public GetUserBookingsQueryHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _queryHandler = new GetUserBookingsQueryHandler(DbContext);
    }

    [Fact]
    public async Task Should_Return_UserBookings()
    {
        const int bookingCount = 3;
        var bookings = await CreateBookingsAsync(bookingCount);
        var response = await _queryHandler.Handle(new GetUserBookingsQuery(bookings.UserId), CancellationToken.None);
        response.Should().HaveCount(bookingCount);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException_WhenUserDoesNotExist()
    {
        const int notExistingId = 999;
        await Assert.ThrowsAsync<UserNotFoundException>(() => _queryHandler.Handle(new GetUserBookingsQuery(notExistingId), CancellationToken.None));
    }
}