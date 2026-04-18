using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Application.Mappings;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.FullName, user.Email, user.Role?.Name ?? string.Empty, user.CreatedAtUtc);

    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Description);

    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.CreatedAtUtc);
}
