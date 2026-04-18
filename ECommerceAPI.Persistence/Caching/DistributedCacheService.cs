using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ECommerceAPI.Application.Abstractions.Caching;

namespace ECommerceAPI.Persistence.Caching;

public sealed class DistributedCacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await distributedCache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedValue))
        {
            var deserialized = JsonSerializer.Deserialize<T>(cachedValue, SerializerOptions);
            if (deserialized is not null)
            {
                return deserialized;
            }
        }

        var value = await factory(cancellationToken);
        var serialized = JsonSerializer.Serialize(value, SerializerOptions);

        await distributedCache.SetStringAsync(
            key,
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        distributedCache.RemoveAsync(key, cancellationToken);

    public async Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken = default)
    {
        var rawValue = await distributedCache.GetStringAsync(GetVersionKey(scope), cancellationToken);
        return long.TryParse(rawValue, out var version) ? version : 1L;
    }

    public async Task<long> IncrementVersionAsync(string scope, CancellationToken cancellationToken = default)
    {
        var nextVersion = await GetVersionAsync(scope, cancellationToken) + 1;

        await distributedCache.SetStringAsync(
            GetVersionKey(scope),
            nextVersion.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            cancellationToken);

        return nextVersion;
    }

    private static string GetVersionKey(string scope) => $"cache:version:{scope}";
}
