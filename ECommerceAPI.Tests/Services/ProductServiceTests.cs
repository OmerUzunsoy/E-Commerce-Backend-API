using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ECommerceAPI.Application.Abstractions.Caching;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.Common.Models;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Options;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Services;
using ECommerceAPI.Tests.Helpers;

namespace ECommerceAPI.Tests.Services;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistProduct_WhenRequestIsValid()
    {
        await using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Electronics", Description = "Devices" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var cache = CreateCacheMock();
        var sut = new ProductService(context, cache.Object, Options.Create(new CacheOptions()));

        var result = await sut.CreateAsync(new CreateProductRequestDto("Keyboard", "Wireless", 99.99m, 8, category.Id));

        result.Name.Should().Be("Keyboard");
        context.Products.Should().ContainSingle(x => x.Name == "Keyboard");
        cache.Verify(x => x.IncrementVersionAsync("products", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterBySearchAndPriceRange()
    {
        await using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Electronics", Description = "Devices" };
        context.Categories.Add(category);
        context.Products.AddRange(
            new Product { Name = "Gaming Mouse", Description = "Mouse", Price = 40m, Stock = 5, Category = category },
            new Product { Name = "Gaming Keyboard", Description = "Keyboard", Price = 120m, Stock = 5, Category = category },
            new Product { Name = "Desk Lamp", Description = "Lamp", Price = 30m, Stock = 5, Category = category });
        await context.SaveChangesAsync();

        var sut = new ProductService(context, CreatePassthroughCacheMock<PagedResult<ProductDto>>().Object, Options.Create(new CacheOptions()));

        var result = await sut.GetAllAsync(new ProductQueryParameters
        {
            Search = "gaming",
            MinPrice = 50m,
            MaxPrice = 150m
        });

        result.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Gaming Keyboard");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var sut = new ProductService(context, CreatePassthroughCacheMock<ProductDto>().Object, Options.Create(new CacheOptions()));

        var action = () => sut.GetByIdAsync(Guid.NewGuid());

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product not found.");
    }

    private static Mock<ICacheService> CreateCacheMock()
    {
        var mock = new Mock<ICacheService>();
        mock.Setup(x => x.GetVersionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(1L);
        mock.Setup(x => x.IncrementVersionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(2L);
        return mock;
    }

    private static Mock<ICacheService> CreatePassthroughCacheMock<T>()
    {
        var mock = CreateCacheMock();
        mock.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<Func<CancellationToken, Task<T>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, TimeSpan, Func<CancellationToken, Task<T>>, CancellationToken>((_, _, factory, token) => factory(token));

        return mock;
    }
}
