using Microsoft.Extensions.Logging;

namespace EventService.Infrastructure.Logging;

public static partial class EventLogs
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Получено событие бронирования: BookingId={BookingId}, EventId={EventId}")]
    public static partial void LogBookingReceived(this ILogger logger, int bookingId, int eventId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Событие бронирования обработано: BookingId={BookingId}")]
    public static partial void LogBookingProcessed(this ILogger logger, int bookingId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Ошибка при обработке события бронирования: BookingId={BookingId}")]
    public static partial void LogBookingProcessingError(this ILogger logger, Exception exception, int bookingId);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Повторная попытка {RetryCount} через {DelaySeconds}с при обработке события бронирования")]
    public static partial void LogRetry(this ILogger logger, int retryCount, int delaySeconds);
    
    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Warning,
        Message = "Повторная попытка {RetryCount} через {DelaySeconds}с при отправке события")]
    public static partial void LogPublishRetry(this ILogger logger, int retryCount, int delaySeconds, Exception exception);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Warning,
        Message = "Событие {EventId} не найдено для брони {BookingId}")]
    public static partial void LogEventNotFound(this ILogger logger, int eventId, int bookingId);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Information,
        Message = "Бронь {BookingId} подтверждена для события {EventId}")]
    public static partial void LogBookingConfirmed(this ILogger logger, int bookingId, int eventId);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Warning,
        Message = "Не удалось зарезервировать место для брони {BookingId}: {Reason}")]
    public static partial void LogReservationFailed(this ILogger logger, int bookingId, string reason);

    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Information,
        Message = "Места возвращены для события {EventId} (бронь {BookingId})")]
    public static partial void LogSeatsReleased(this ILogger logger, int eventId, int bookingId);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Information,
        Message = "Получено событие отмены брони: BookingId={BookingId}")]
    public static partial void LogBookingCancelledReceived(this ILogger logger, int bookingId);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Warning,
        Message = "Событие {EventId} уже было обработано, пропускаем")]
    public static partial void LogEventAlreadyProcessed(this ILogger logger, string eventId);

    [LoggerMessage(
        EventId = 2012,
        Level = LogLevel.Information,
        Message = "Событие {EventId} сохранено в Inbox")]
    public static partial void LogEventSavedToInbox(this ILogger logger, string eventId);

    [LoggerMessage(
        EventId = 2013,
        Level = LogLevel.Information,
        Message = "Созданы топики: {Topics}")]
    public static partial void LogTopicsCreated(this ILogger logger, string topics);

    [LoggerMessage(
        EventId = 2014,
        Level = LogLevel.Information,
        Message = "Все топики уже существуют")]
    public static partial void LogTopicsAlreadyExist(this ILogger logger);

    [LoggerMessage(
        EventId = 2015,
        Level = LogLevel.Error,
        Message = "Ошибка при создании топиков Kafka")]
    public static partial void LogTopicsCreationError(this ILogger logger, Exception exception);
}