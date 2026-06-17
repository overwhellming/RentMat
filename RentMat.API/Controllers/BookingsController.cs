using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Booking;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Core.Enums;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    
    private readonly BookDeviceHandler _bookDeviceHandler;
    private readonly CompleteBookingHandler _completeBookingHandler;
    private readonly GetBookingsHandler _getBookingsHandler;

    public BookingsController(BookDeviceHandler bookDeviceHandler, CompleteBookingHandler completeBookingHandler,
        GetBookingsHandler getBookingsHandler)
    {
        _bookDeviceHandler = bookDeviceHandler;
        _completeBookingHandler = completeBookingHandler;
        _getBookingsHandler = getBookingsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] BookingStatus? status = null)
    {
        return Ok(await _getBookingsHandler.Handle(page, pageSize, search, status, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto, CancellationToken cancellationToken)
    {
        var response = await _bookDeviceHandler.Handle(dto, cancellationToken);
        return Ok(response);
    }

    [HttpPost("complete/{deviceId:int}")]
    public async Task<IActionResult> CompleteBooking(int deviceId, CancellationToken cancellationToken)
    {
        await _completeBookingHandler.Handle(deviceId, cancellationToken);
        return NoContent();
    }
}