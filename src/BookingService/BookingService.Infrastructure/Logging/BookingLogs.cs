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
    
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Получено событие подтверждения брони: BookingId={BookingId}")]
    public static partial void LogBookingConfirmedReceived(this ILogger logger, int bookingId);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Бронь {BookingId} подтверждена")]
    public static partial void LogBookingConfirmedProcessed(this ILogger logger, int bookingId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "Получено событие отклонения брони: BookingId={BookingId}, Причина={Reason}")]
    public static partial void LogBookingRejectedReceived(this ILogger logger, int bookingId, string reason);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "Бронь {BookingId} отклонена. Причина: {Reason}")]
    public static partial void LogBookingRejectedProcessed(this ILogger logger, int bookingId, string reason);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Warning,
        Message = "Повторная попытка {RetryCount} через {DelaySeconds}с при обработке события")]
    public static partial void LogRetry(this ILogger logger, int retryCount, int delaySeconds);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Warning,
        Message = "Бронь {BookingId} не найдена")]
    public static partial void LogBookingNotFound(this ILogger logger, int bookingId);
}