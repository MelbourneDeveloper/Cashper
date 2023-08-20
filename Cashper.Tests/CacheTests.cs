using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Cashper.Tests;

[TestClass]
public class CacheTests
{
    private static byte[] _data = new byte[] { 1, 2, 3 };

    [TestMethod]
    public async Task VerifyDistributedCacheGetsOneHit()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        mockDistributedCache
            .Setup(c => c.GetAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_data);
        var cache = new DistributedCacheWithMemoryCaching(
            mockDistributedCache.Object,
            new MemoryCache(new MemoryCacheOptions()),
            new BasicAdapter()
        );
        var data = await cache.GetAsync("123");
        Assert.IsTrue(data.SequenceEqual(_data));
        data = await cache.GetAsync("123");
        Assert.IsTrue(data.SequenceEqual(_data));
        mockDistributedCache.Verify(
            c => c.GetAsync("123", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [TestMethod]
    public async Task VerifySerialization()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        mockDistributedCache
            .Setup(c => c.GetAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TestModel("test"))));
        var cache = new DistributedCacheWithMemoryCaching(
            mockDistributedCache.Object,
            new MemoryCache(new MemoryCacheOptions()),
            new BasicAdapter()
        );
        var slowThing = await cache.GetAsync<TestModel>("123");
        Assert.AreEqual("test", slowThing.Test);
        slowThing = await cache.GetAsync<TestModel>("123");
        Assert.AreEqual("test", slowThing.Test);
        mockDistributedCache.Verify(
            c => c.GetAsync("123", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [TestMethod]
    public async Task VerifyGetOrCreateAsync()
    {
        var expectedBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(await GetTestModel("123"))
        );

        var mockDistributedCache = new FakeCache();
        var cache = new DistributedCacheWithMemoryCaching(
            mockDistributedCache,
            new MemoryCache(new MemoryCacheOptions()),
            new BasicAdapter()
        );

        var slowThing = await cache.GetOrCreateAsync("123", GetTestModel);
        Assert.AreEqual("test2", slowThing.Test);
        slowThing = await cache.GetOrCreateAsync("123", GetTestModel);
        Assert.AreEqual("test2", slowThing.Test);
        Assert.AreEqual(0, mockDistributedCache.GetCount);
        Assert.IsTrue(expectedBytes.SequenceEqual(mockDistributedCache.Values["123"]));
    }

    async Task<TestModel> GetTestModel(string key) => new TestModel("test2");
}
