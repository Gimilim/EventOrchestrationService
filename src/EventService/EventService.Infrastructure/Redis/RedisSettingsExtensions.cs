using EventService.Application.Settings;
using StackExchange.Redis;

namespace EventService.Infrastructure.Redis;

public static class RedisSettingsExtensions
{
    public static ConfigurationOptions ToConfigurationOptions(this RedisSettings settings)
    {
        return new ConfigurationOptions
        {
            EndPoints = { { settings.Host, settings.Port } },
            Password = settings.Password,
            ConnectTimeout = settings.ConnectTimeout,
            SyncTimeout = settings.SyncTimeout,
            AbortOnConnectFail = settings.AbortOnConnectFail,
        };
    }

    public static TimeSpan GetDefaultCacheTtl(this RedisSettings settings)
    {
        return TimeSpan.FromMinutes(settings.DefaultCacheTtlMinutes);
    }
}