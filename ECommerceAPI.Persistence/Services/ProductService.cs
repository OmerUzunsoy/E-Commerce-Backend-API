using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ECommerceAPI.Application.Abstractions.Caching;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.Common.Models;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Mappings;
using ECommerceAPI.Application.Options;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Caching;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class ProductService(
    ECommerceDbContext context,
    ICacheService cacheService,
    IOptions<CacheOptions> cacheOptions) : IProductService
{
    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters query, CancellationToken cancellationToken = default)
    {
        var version = await cacheService.GetVersionAsync("products", cancellationToken);
        return await cacheService.GetOrCreateAsync(
            CacheKeys.ProductList(query, version),
            TimeSpan.FromMinutes(cacheOptions.Value.ProductTtlMinutes),
            async token => await BuildPagedProductsAsync(query, token),
            cancellationToken);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var version = await cacheService.GetVersionAsync("products", cancellationToken);
        return await cacheService.GetOrCreateAsync(
            CacheKeys.ProductDetail(id, version),
            TimeSpan.FromMinutes(cacheOptions.Value.ProductTtlMinutes),
            async token =>
            {
                var product = await context.Products
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id, token)
                    ?? throw new NotFoundException("Product not found.");

                return product.ToDto();
            },
            cancellationToken);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = category.Id,
            Category = category
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);

        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = category.Id;
        product.Category = category;

        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);
        return product.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);
    }

    private async Task<PagedResult<ProductDto>> BuildPagedProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken)
    {
        var productsQuery = context.Products
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(x => x.Name.ToLower().Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.Price <= query.MaxPrice.Value);
        }

        productsQuery = (query.SortBy?.ToLowerInvariant(), query.Descending) switch
        {
            ("price", false) => productsQuery.OrderBy(x => x.Price),
            ("price", true) => productsQuery.OrderByDescending(x => x.Price),
            ("name", false) => productsQuery.OrderBy(x => x.Name),
            ("name", true) => productsQuery.OrderByDescending(x => x.Name),
            ("createdat", false) => productsQuery.OrderBy(x => x.CreatedAtUtc),
            _ => productsQuery.OrderByDescending(x => x.CreatedAtUtc)
        };

        var totalCount = await productsQuery.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var pageNumber = Math.Max(1, query.PageNumber);

        var items = await productsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductDto(
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.Stock,
                x.CategoryId,
                x.Category!.Name,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
