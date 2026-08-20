using System.Text.Json;
using EventService.Application.Interfaces;
using EventService.Application.Logging;
using EventService.Application.Settings;
using EventService.Infrastructure.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EventService.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger, IOptions<RedisSettings> redisSettings)
    : ICacheService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            logger.LogCacheGetError(ex, key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            var defaultCacheTtl = redisSettings.Value.GetDefaultCacheTtl();
            await _database.StringSetAsync(key, json, expiration ?? defaultCacheTtl);
        }
        catch (Exception ex)
        {
            logger.LogCacheSetError(ex, key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogCacheRemoveError(ex, key);
        }
    }
}