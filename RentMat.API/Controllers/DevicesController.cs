using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Devices;
using RentMat.Core.Enums;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly GetDevicesHandler _devicesHandler;

    public DevicesController(GetDevicesHandler devicesHandler)
    {
        _devicesHandler = devicesHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null,
        [FromQuery] DeviceStatus? status = null)
    {
        return Ok(await _devicesHandler.Handle(page, pageSize, search, status, cancellationToken));
    }
}