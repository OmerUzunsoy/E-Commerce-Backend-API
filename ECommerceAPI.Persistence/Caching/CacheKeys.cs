using System.Security.Cryptography;
using System.Text;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Persistence.Caching;

internal static class CacheKeys
{
    public static string CategoryList(long version) => $"categories:v{version}:list";

    public static string ProductList(ProductQueryParameters query, long version)
    {
        var normalized = string.Join(
            '|',
            version,
            query.PageNumber,
            query.PageSize,
            query.Search?.Trim().ToLowerInvariant() ?? string.Empty,
            query.CategoryId?.ToString() ?? string.Empty,
            query.MinPrice?.ToString() ?? string.Empty,
            query.MaxPrice?.ToString() ?? string.Empty,
            query.SortBy?.Trim().ToLowerInvariant() ?? string.Empty,
            query.Descending);

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
        return $"products:v{version}:list:{hash}";
    }

    public static string ProductDetail(Guid id, long version) => $"products:v{version}:detail:{id}";
}
