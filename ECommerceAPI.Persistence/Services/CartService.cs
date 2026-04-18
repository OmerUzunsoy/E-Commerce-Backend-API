using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class CartService(ECommerceDbContext context) : ICartService
{
    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return MapCart(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.Stock < request.Quantity)
        {
            throw new AppException("Insufficient stock.");
        }

        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (existingItem is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = request.Quantity
            });
        }
        else
        {
            if (product.Stock < existingItem.Quantity + request.Quantity)
            {
                throw new AppException("Insufficient stock.");
            }

            existingItem.Quantity += request.Quantity;
        }

        await context.SaveChangesAsync(cancellationToken);
        return MapCart(cart);
    }

    public async Task<CartDto> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new NotFoundException("Cart item not found.");

        if (item.Product!.Stock < request.Quantity)
        {
            throw new AppException("Insufficient stock.");
        }

        item.Quantity = request.Quantity;
        await context.SaveChangesAsync(cancellationToken);

        return MapCart(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new NotFoundException("Cart item not found.");

        context.CartItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);

        return await GetCartAsync(userId, cancellationToken);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userExists = await context.Users.AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }

        var cart = await context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        context.Carts.Add(cart);
        await context.SaveChangesAsync(cancellationToken);

        return await context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstAsync(x => x.Id == cart.Id, cancellationToken);
    }

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.Items
            .Select(x => new CartItemDto(
                x.Id,
                x.ProductId,
                x.Product?.Name ?? string.Empty,
                x.Product?.Price ?? 0m,
                x.Quantity,
                (x.Product?.Price ?? 0m) * x.Quantity))
            .ToList();

        return new CartDto(cart.Id, cart.UserId, items, items.Sum(x => x.LineTotal));
    }
}
