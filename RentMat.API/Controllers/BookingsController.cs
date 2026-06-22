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

    private readonly DeviceBookingHandler _deviceBookingHandler;
    private readonly CompleteBookingHandler _completeBookingHandler;
    private readonly GetAllBookingsHandler _getAllBookingsHandler;
    private readonly GetUserBookingsHandler _getUserBookingsHandler;

    public BookingsController(DeviceBookingHandler deviceBookingHandler, CompleteBookingHandler completeBookingHandler,
        GetAllBookingsHandler getAllBookingsHandler, GetUserBookingsHandler getUserBookingsHandler)
    {
        _deviceBookingHandler = deviceBookingHandler;
        _completeBookingHandler = completeBookingHandler;
        _getAllBookingsHandler = getAllBookingsHandler;
        _getUserBookingsHandler = getUserBookingsHandler;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] BookingStatus? status = null)
    {
        return Ok(await _getAllBookingsHandler.Handle(page, pageSize, search, status, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return Ok(
            await _getUserBookingsHandler.Handle(userId, cancellationToken));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingCreateDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return Ok(await _deviceBookingHandler.Handle(dto, userId, cancellationToken));
    }

    [Authorize]
    [HttpPost("complete/{deviceId:int}")]
    public async Task<IActionResult> Complete(int deviceId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _completeBookingHandler.Handle(deviceId, userId, cancellationToken);
        return NoContent();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User ID claim is missing");
        return int.Parse(userIdClaim);
    }
}