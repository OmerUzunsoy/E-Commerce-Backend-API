using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ECommerceAPI.Application.Abstractions.Caching;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Mappings;
using ECommerceAPI.Application.Options;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Caching;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class CategoryService(
    ECommerceDbContext context,
    ICacheService cacheService,
    IOptions<CacheOptions> cacheOptions) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var version = await cacheService.GetVersionAsync("categories", cancellationToken);
        return await cacheService.GetOrCreateAsync(
            CacheKeys.CategoryList(version),
            TimeSpan.FromMinutes(cacheOptions.Value.CategoryTtlMinutes),
            async token =>
            {
                var categories = await context.Categories.OrderBy(x => x.Name).ToListAsync(token);
                return (IReadOnlyCollection<CategoryDto>)categories.Select(x => x.ToDto()).ToList();
            },
            cancellationToken);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var exists = await context.Categories.AnyAsync(x => x.Name == request.Name, cancellationToken);
        if (exists)
        {
            throw new AppException("Category name already exists.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim()
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("categories", cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        category.Name = request.Name.Trim();
        category.Description = request.Description.Trim();
        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("categories", cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);

        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.Products.Count != 0)
        {
            throw new AppException("Category cannot be deleted while products exist.");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        await cacheService.IncrementVersionAsync("categories", cancellationToken);
        await cacheService.IncrementVersionAsync("products", cancellationToken);
    }
}
