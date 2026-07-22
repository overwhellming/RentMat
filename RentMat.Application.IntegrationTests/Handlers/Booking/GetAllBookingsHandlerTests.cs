using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentMat.Application.Common;
using RentMat.Application.Handlers.Booking;
using RentMat.Application.Handlers.Devices;
using RentMat.Application.IntegrationTests.Infrastructure;
using RentMat.Application.Queries;
using RentMat.Core.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace RentMat.Application.IntegrationTests.Handlers.Booking;

[Collection("Integration Tests Collection")]
public class GetAllBookingsHandlerTests : BaseIntegrationTest
{
    private const string Name1 = "Laptop";
    private const string Name2 = "Phone";
    private const string Name3 = "Console";
    
    private readonly GetAllBookingsHandler _handler;
    private readonly IFusionCache _cache;
    
    public GetAllBookingsHandlerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        using var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IFusionCache>();
        _handler = new GetAllBookingsHandler(DbContext, _cache,
            scope.ServiceProvider.GetRequiredService<ILogger<GetAllBookingsHandler>>());
    }

    [Fact]
    public async Task Should_Return_EmptyList_When_There_Is_NoBookings()
    {
        var result = await _handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_PaginatedResult()
    {
        const int bookingsAmount = 15;
        await CreateBookingsAsync(amount: bookingsAmount);

        const int pageSize = 10;
        var result = await _handler.Handle(new GetAllBookingsQuery(Page: 1, PageSize: pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(10);

        result = await _handler.Handle(new GetAllBookingsQuery(Page: 2, PageSize: pageSize),
            CancellationToken.None);
        result.Items.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("la", 1)]
    [InlineData(Name1, 1)]
    [InlineData(Name2, 1)]
    [InlineData("Not found", 0)]
    public async Task Should_Return_FilteredResult_By_Search(string search, int expectedCount)
    {
        await CreateBookingAsync(deviceName: Name1);
        await CreateBookingAsync(deviceName: Name2);
        await CreateBookingAsync(deviceName: Name3);

        var result = await _handler.Handle(new GetAllBookingsQuery(Search: search),
            CancellationToken.None);
        result.Items.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Should_Return_FilteredResult_By_Status()
    {
        const BookingStatus status1 = BookingStatus.Active;
        const BookingStatus status2 = BookingStatus.Cancelled;

        await CreateBookingAsync(status: status1);
        await CreateBookingAsync(status: status2);

        var result = await _handler.Handle(new GetAllBookingsQuery(Status: status1), 
            CancellationToken.None);
        result.Items.Should().HaveCount(1);
        result.Items.Select(u => u.StatusName).Should().BeEquivalentTo(status1.ToString());
    }

    [Theory]
    [InlineData(GetAllBookingsHandler.MaxPageSize + 1, GetAllBookingsHandler.MaxPageSize)]
    [InlineData(0, GetAllBookingsHandler.DefaultPageSize)]
    [InlineData(25, 25)]
    public async Task Should_Apply_CorrectPageSize(int inputPageSize, int expectedPageSize)
    {
        var result = await _handler.Handle(new GetAllBookingsQuery(PageSize: inputPageSize), 
            CancellationToken.None);
        result.PageSize.Should().Be(expectedPageSize);
    }
    
    [Fact]
    public async Task Should_Constraint_Min_Page()
    {
        var result = await _handler.Handle(new GetAllBookingsQuery(Page: 0 ),
            CancellationToken.None);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Should_Return_CachedBookings_If_CacheExists()
    {
        const int bookingsAmount = 5;
        await CreateBookingsAsync(amount: bookingsAmount);

        var result = await _handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(bookingsAmount);

        await CreateBookingAsync();
        
        result = await _handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(bookingsAmount);
    }
    
    [Fact]
    public async Task Should_Return_UpdatedData_After_CacheInvalidation()
    {
        const int bookingsAmount = 5;
        await CreateBookingsAsync(amount: bookingsAmount);

        var result = await _handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(bookingsAmount);

        await CreateBookingAsync();
        await _cache.RemoveByTagAsync(CacheTags.Bookings);
        
        result = await _handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);
        result.TotalItems.Should().Be(bookingsAmount + 1);
    }
}