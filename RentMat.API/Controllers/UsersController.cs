using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.User;
using RentMat.Application.Handlers.Users;
using RentMat.Core.Enums;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;

    private readonly GetAllUsersHandler _allUsersHandler;
    private readonly GetUserByIdHandler _getUserByIdHandler;
    private readonly DepositUserBalanceHandler _depositUserBalanceHandler;
    private readonly GetUserBalanceHandler _getUserBalanceHandler;

    public UsersController(GetUserBalanceHandler getUserBalanceHandler,
        DepositUserBalanceHandler depositUserBalanceHandler, GetAllUsersHandler allUsersHandler,
        GetUserByIdHandler getUserByIdHandler)
    {
        _getUserBalanceHandler = getUserBalanceHandler;
        _depositUserBalanceHandler = depositUserBalanceHandler;
        _allUsersHandler = allUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null)
    {
        return Ok(await _allUsersHandler.Handle(page, pageSize, search, role, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _getUserByIdHandler.Handle(id, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        return Ok(await _getUserByIdHandler.Handle(GetUserId(), cancellationToken));
    }

    [Authorize]
    [HttpGet("me/balance")]
    public async Task<IActionResult> GetMyBalance(CancellationToken cancellationToken)
    {
        return Ok(await _getUserBalanceHandler.Handle(GetUserId(), cancellationToken));
    }

    [Authorize]
    [HttpPost("me/balance")]
    public async Task<IActionResult> Deposit([FromBody] DepositCreateDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _depositUserBalanceHandler.Handle(dto.Amount, GetUserId(), cancellationToken));
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User ID claim is missing");
        return int.Parse(userIdClaim);
    }
}