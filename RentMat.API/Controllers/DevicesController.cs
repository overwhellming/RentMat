using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Handlers.Devices;
using RentMat.Core.Enums;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    
    private readonly GetDevicesHandler _devicesHandler;

    public DevicesController(GetDevicesHandler devicesHandler)
    {
        _devicesHandler = devicesHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage, 
        [FromQuery] int pageSize = DefaultPageSize, 
        [FromQuery] string? search = null,
        [FromQuery] DeviceStatus? status = null)
    {
        return Ok(await _devicesHandler.Handle(page, pageSize, search, status, cancellationToken));
    }
}