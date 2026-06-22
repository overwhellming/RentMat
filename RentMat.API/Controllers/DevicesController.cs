using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.Device;
using RentMat.Application.Handlers.Devices;
using RentMat.Core.Enums;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    
    private readonly GetAllDevicesHandler _allDevicesHandler;
    private readonly GetDeviceByIdHandler _deviceByIdHandler;
    private readonly UpdateDeviceHandler _updateDeviceHandler;
    
    public DevicesController(GetAllDevicesHandler allDevicesHandler, GetDeviceByIdHandler deviceByIdHandler, UpdateDeviceHandler updateDeviceHandler)
    {
        _allDevicesHandler = allDevicesHandler;
        _deviceByIdHandler = deviceByIdHandler;
        _updateDeviceHandler = updateDeviceHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage, 
        [FromQuery] int pageSize = DefaultPageSize, 
        [FromQuery] string? search = null,
        [FromQuery] DeviceStatus? status = null)
    {
        return Ok(await _allDevicesHandler.Handle(page, pageSize, search, status, cancellationToken));
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _deviceByIdHandler.Handle(id, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DeviceUpdateDto dto, CancellationToken cancellationToken)
    {
        await _updateDeviceHandler.Handle(id, dto, cancellationToken);
        return NoContent();
    }
}