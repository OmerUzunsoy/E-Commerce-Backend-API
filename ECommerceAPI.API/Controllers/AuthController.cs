using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.API.Filters;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilter<RegisterRequestDto>))]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RegisterAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    [ServiceFilter(typeof(ValidationFilter<LoginRequestDto>))]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh-token")]
    [ServiceFilter(typeof(ValidationFilter<RefreshTokenRequestDto>))]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshTokenAsync(request, cancellationToken));
    }

    [HttpPost("logout")]
    [ServiceFilter(typeof(ValidationFilter<RefreshTokenRequestDto>))]
    public async Task<IActionResult> Logout(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }
}
