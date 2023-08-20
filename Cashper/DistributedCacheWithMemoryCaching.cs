using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Cashper;

public class DistributedCacheWithMemoryCaching : IDistributedCache
{
    private IDistributedCache _distributedCache;
    private IMemoryCache _memoryCache;
    private ISerializationAdapter _serializationAdapter;

    public DistributedCacheWithMemoryCaching(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ISerializationAdapter serializationAdapter
    )
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _serializationAdapter = serializationAdapter;
    }

    public byte[]? Get(string key) =>
        _memoryCache.GetOrCreate(key, entry => _distributedCache.Get(key));

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        _memoryCache.GetOrCreateAsync(key, entry => _distributedCache.GetAsync(key, token));

    public Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>
        _memoryCache.GetOrCreateAsync(
            key,
            async (entry) =>
                _serializationAdapter.Deserialize<T>(
                    key,
                    await _distributedCache.GetAsync(key, token)
                )
        );

    public async Task<T> GetOrCreateAsync<T>(string key, Func<string, Task<T>> factory)
    {
        var value = await GetAsync<T>(key);

        if (value != null)
        {
            return value;
        }

        value = await factory(key);

        await SetAsync(
            key,
            _serializationAdapter.Serialize(value),
            new DistributedCacheEntryOptions()
        );

        return value;
    }

    public void Refresh(string key) => _distributedCache.Refresh(key);

    public Task RefreshAsync(string key, CancellationToken token = default) =>
        _distributedCache.RefreshAsync(key, token);

    public void Remove(string key) => _distributedCache.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default) =>
        _distributedCache.RemoveAsync(key, token);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        _distributedCache.Set(key, value, options);

    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default
    ) => _distributedCache.SetAsync(key, value, options, token);
}
