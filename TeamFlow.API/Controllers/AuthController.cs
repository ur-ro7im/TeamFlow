using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamFlow.API.DTOs.Auth;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Register(
        RegisterDto dto)
    {
        var result =
            await _authService.RegisterAsync(dto);

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login(
        LoginDto dto)
    {
        var result =
            await _authService.LoginAsync(dto);

        return Ok(result);
    }
    [HttpPost("register-from-invitation")]
    [AllowAnonymous]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult>
    RegisterFromInvitation(
        RegisterFromInvitationDto dto)
    {
        var result =
            await _authService
                .RegisterFromInvitationAsync(dto);

        return Ok(result);
    }
}