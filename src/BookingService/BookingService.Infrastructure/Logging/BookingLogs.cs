using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Logging;

public static partial class BookingLogs
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Бронь {BookingId} помечена как Failed после {RetryCount} попыток")]
    public static partial void LogFailedBooking(this ILogger logger, int bookingId, int retryCount);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Повторная отправка события для брони {BookingId}, попытка {RetryCount}")]
    public static partial void LogBookingRetry(this ILogger logger, int bookingId, int retryCount);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Ошибка при отправке события для брони {BookingId}")]
    public static partial void LogBookingPublishError(this ILogger logger, Exception exception, int bookingId);
    
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Повторная попытка {RetryCount} через {DelaySeconds}с при отправке события бронирования")]
    public static partial void LogPublishRetry(this ILogger logger, int retryCount, int delaySeconds, Exception exception);
}