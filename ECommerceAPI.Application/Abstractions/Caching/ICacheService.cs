namespace ECommerceAPI.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken = default);
    Task<long> IncrementVersionAsync(string scope, CancellationToken cancellationToken = default);
}
