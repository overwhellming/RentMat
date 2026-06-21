using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.DTOs.Authentication;
using RentMat.Application.Handlers.Authentication;

namespace RentMat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    private readonly RegisterHandler _registerHandler;

    public AuthController(RegisterHandler registerHandler, LoginHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        await _registerHandler.Handle(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var token = await _loginHandler.Handle(dto, cancellationToken);
        return Ok(new { token });
    }
}