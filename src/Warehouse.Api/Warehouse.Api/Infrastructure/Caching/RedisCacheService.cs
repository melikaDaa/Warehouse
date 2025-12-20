using Microsoft.Extensions.Caching.Distributed;
using Microsoft.FeatureManagement;
using System.Text.Json;
using Warehouse.Api.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        IFeatureManager featureManager,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _featureManager = featureManager;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (!await _featureManager.IsEnabledAsync("UseRedisCache"))
        {
            _logger.LogInformation("Redis disabled. Skip cache for key: {Key}", key);
            return default;
        }

        var json = await _cache.GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogInformation("Cache MISS for key: {Key}", key);
            return default;
        }

        _logger.LogInformation("Cache HIT for key: {Key}", key);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        if (!await _featureManager.IsEnabledAsync("UseRedisCache"))
            return;

        _logger.LogInformation("Cache SET for key: {Key}", key);

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
    }

    public async Task RemoveAsync(string key)
    {
        if (!await _featureManager.IsEnabledAsync("UseRedisCache"))
            return;

        _logger.LogInformation("Cache REMOVE for key: {Key}", key);
        await _cache.RemoveAsync(key);
    }
}
