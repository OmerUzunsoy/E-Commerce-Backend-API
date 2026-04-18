using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class OrderService(ECommerceDbContext context) : IOrderService
{
    public async Task<OrderDto> CreateOrderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Cart not found.");

        if (cart.Items.Count == 0)
        {
            throw new AppException("Cart is empty.");
        }

        foreach (var item in cart.Items)
        {
            if (item.Product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            if (item.Product.Stock < item.Quantity)
            {
                throw new AppException($"Insufficient stock for product '{item.Product.Name}'.");
            }
        }

        var order = new Order
        {
            UserId = userId,
            TotalAmount = cart.Items.Sum(x => x.Product!.Price * x.Quantity),
            Items = cart.Items.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.Product!.Price
            }).ToList()
        };

        foreach (var item in cart.Items)
        {
            item.Product!.Stock -= item.Quantity;
        }

        context.Orders.Add(order);
        context.CartItems.RemoveRange(cart.Items);
        await context.SaveChangesAsync(cancellationToken);

        return await GetOrderByIdAsync(order.Id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await QueryOrders()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryOrders()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        order.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        return await GetOrderByIdAsync(orderId, cancellationToken);
    }

    private async Task<OrderDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await QueryOrders()
            .FirstAsync(x => x.Id == orderId, cancellationToken);
    }

    private IQueryable<OrderDto> QueryOrders()
    {
        return context.Orders
            .Include(x => x.User)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Select(x => new OrderDto(
                x.Id,
                x.UserId,
                x.User!.FullName,
                x.TotalAmount,
                x.Status,
                x.CreatedAtUtc,
                x.Items.Select(item => new OrderItemDto(
                    item.Id,
                    item.ProductId,
                    item.Product!.Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.UnitPrice * item.Quantity)).ToList()));
    }
}
