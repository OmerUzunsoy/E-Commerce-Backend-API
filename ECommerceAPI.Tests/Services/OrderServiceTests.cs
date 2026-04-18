using FluentAssertions;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;
using ECommerceAPI.Persistence.Services;
using ECommerceAPI.Tests.Helpers;

namespace ECommerceAPI.Tests.Services;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCartIsEmpty()
    {
        await using var context = TestDbContextFactory.Create();
        var role = new Role { Name = "Customer" };
        var user = new User { FullName = "Jane Doe", Email = "jane@example.com", PasswordHash = "hash", Role = role };
        var cart = new Cart { User = user };
        context.AddRange(role, user, cart);
        await context.SaveChangesAsync();

        var sut = new OrderService(context);

        var action = () => sut.CreateOrderAsync(user.Id);

        await action.Should().ThrowAsync<AppException>().WithMessage("Cart is empty.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenStockIsInsufficient()
    {
        await using var context = TestDbContextFactory.Create();
        var setup = await CreateOrderSetupAsync(context, stock: 1, quantity: 2, price: 50m);
        var sut = new OrderService(context);

        var action = () => sut.CreateOrderAsync(setup.user.Id);

        await action.Should().ThrowAsync<AppException>()
            .WithMessage("Insufficient stock*");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCalculateTotalAndReduceStock()
    {
        await using var context = TestDbContextFactory.Create();
        var setup = await CreateOrderSetupAsync(context, stock: 10, quantity: 3, price: 25m);
        var sut = new OrderService(context);

        var result = await sut.CreateOrderAsync(setup.user.Id);

        result.TotalAmount.Should().Be(75m);
        result.Items.Should().ContainSingle();
        (await context.Products.FindAsync(setup.product.Id))!.Stock.Should().Be(7);
        context.CartItems.Should().BeEmpty();
    }

    private static async Task<(User user, Product product)> CreateOrderSetupAsync(ECommerceDbContext context, int stock, int quantity, decimal price)
    {
        var role = new Role { Name = "Customer" };
        var user = new User { FullName = "Jane Doe", Email = $"{Guid.NewGuid()}@example.com", PasswordHash = "hash", Role = role };
        var category = new Category { Name = $"Category-{Guid.NewGuid()}", Description = "Category" };
        var product = new Product { Name = "Keyboard", Description = "Wireless", Price = price, Stock = stock, Category = category };
        var cart = new Cart { User = user };
        var item = new CartItem { Cart = cart, Product = product, Quantity = quantity };

        context.AddRange(role, user, category, product, cart, item);
        await context.SaveChangesAsync();

        return (user, product);
    }
}
