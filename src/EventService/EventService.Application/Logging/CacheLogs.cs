using Microsoft.Extensions.Logging;

namespace EventService.Application.Logging;

public static partial class CacheLogs
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "Ошибка при получении значения из Redis по ключу {Key}")]
    public static partial void LogCacheGetError(this ILogger logger, Exception exception, string key);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Error,
        Message = "Ошибка при записи значения в Redis по ключу {Key}")]
    public static partial void LogCacheSetError(this ILogger logger, Exception exception, string key);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Error,
        Message = "Ошибка при удалении значения из Redis по ключу {Key}")]
    public static partial void LogCacheRemoveError(this ILogger logger, Exception exception, string key);
}