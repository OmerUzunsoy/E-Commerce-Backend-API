namespace ECommerceAPI.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAtUtc);

public sealed record CreateProductRequestDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public sealed record UpdateProductRequestDto(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public sealed class ProductQueryParameters
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
