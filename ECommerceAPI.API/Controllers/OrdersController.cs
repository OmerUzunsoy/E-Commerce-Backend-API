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
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<OrderDto>> Create(CancellationToken cancellationToken)
    {
        return Ok(await orderService.CreateOrderAsync(GetUserId(), cancellationToken));
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> MyOrders(CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetMyOrdersAsync(GetUserId(), cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetAllAsync(cancellationToken));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidationFilter<UpdateOrderStatusRequestDto>))]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await orderService.UpdateStatusAsync(id, request, cancellationToken));
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
