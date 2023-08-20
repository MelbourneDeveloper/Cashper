using Microsoft.Extensions.Caching.Distributed;

namespace Cashper.Tests;

public class FakeCache : IDistributedCache
{
    public Dictionary<string, byte[]> Values = new Dictionary<string, byte[]>();
    public int GetCount { get; private set; }

    public byte[]? Get(string key)
    {
        return Values[key];
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        if (!Values.ContainsKey(key))
            return null;
        GetCount++;
        return Values[key];
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        Values[key] = value;
    }

    public async Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default
    )
    {
        Values[key] = value;
    }
}
