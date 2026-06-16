using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Booking;
using RentMat.Application.DTOs.RentalBooking;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookDeviceHandler _bookDeviceHandler;

    public BookingsController(BookDeviceHandler bookDeviceHandler)
    {
        _bookDeviceHandler = bookDeviceHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(CancellationToken cancellationToken, [FromBody] BookingCreateDto dto)
    {
        var response = await _bookDeviceHandler.Handle(dto, cancellationToken);
        return Ok(response);
    }
}