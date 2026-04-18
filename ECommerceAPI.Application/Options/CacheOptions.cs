namespace ECommerceAPI.Application.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";
    public int ProductTtlMinutes { get; set; } = 5;
    public int CategoryTtlMinutes { get; set; } = 10;
}
