using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.RentalBooking;
using RentMat.Application.Handlers.Booking;
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
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var response = await _bookDeviceHandler.Handle(dto, userId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("complete/{deviceId:int}")]
    public async Task<IActionResult> CompleteBooking(int deviceId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _completeBookingHandler.Handle(deviceId, userId, cancellationToken);
        return NoContent();
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
}