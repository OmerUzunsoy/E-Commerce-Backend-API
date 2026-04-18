using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Mappings;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class CategoryService(ECommerceDbContext context) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return categories.Select(x => x.ToDto()).ToList();
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

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        category.Name = request.Name.Trim();
        category.Description = request.Description.Trim();
        await context.SaveChangesAsync(cancellationToken);

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
    }
}
