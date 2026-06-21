using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.User;
using RentMat.Application.Handlers.Users;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DepositUserBalanceHandler _depositUserBalanceHandler;
    private readonly GetUserBalanceHandler _getUserBalanceHandler;

    public UsersController(GetUserBalanceHandler getUserBalanceHandler,
        DepositUserBalanceHandler depositUserBalanceHandler)
    {
        _getUserBalanceHandler = getUserBalanceHandler;
        _depositUserBalanceHandler = depositUserBalanceHandler;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        return Ok(new
        {
            UserId = GetUserId(),
            Login = User.Identity?.Name
        });
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