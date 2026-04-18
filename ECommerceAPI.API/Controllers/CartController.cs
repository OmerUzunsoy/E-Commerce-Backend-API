using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.API.Filters;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer,Admin")]
public sealed class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await cartService.GetCartAsync(GetUserId(), cancellationToken));
    }

    [HttpPost("items")]
    [ServiceFilter(typeof(ValidationFilter<AddCartItemRequestDto>))]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await cartService.AddItemAsync(GetUserId(), request, cancellationToken));
    }

    [HttpPut("items/{id:guid}")]
    [ServiceFilter(typeof(ValidationFilter<UpdateCartItemRequestDto>))]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid id, UpdateCartItemRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await cartService.UpdateItemAsync(GetUserId(), id, request, cancellationToken));
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await cartService.RemoveItemAsync(GetUserId(), id, cancellationToken));
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
