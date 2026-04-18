using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.API.Filters;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        return Ok(await userService.GetProfileAsync(GetUserId(), cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await userService.GetUsersAsync(cancellationToken));
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<UpdateUserRoleRequestDto>))]
    public async Task<ActionResult<UserDto>> UpdateRole(Guid id, UpdateUserRoleRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await userService.UpdateRoleAsync(id, request, cancellationToken));
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
