using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Domain.Enums;
using EventOrchestrationService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventOrchestrationService.API.Controllers;

[ApiController]
[Route("bookings")]
public class BookingController(IBookingService bookingService) : ApiControllerBase
{
    /// <summary>
    /// Получить информацию о бронировании.
    /// </summary>
    /// <param name="id">ID бронирования.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Текущее состояние брони по её идентификатору.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookingStatus(int id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetBookingByIdAsync(id, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException($"Бронирование с ID {id} не найдено");
        }

        return Ok(new { booking.Id, booking.Status });
    }

    /// <summary>
    /// Отменить бронирование.
    /// </summary>
    /// <param name="id">ID бронирования.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Текущее состояние брони по её идентификатору.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> CanselBooking(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();
        var skipCanselPermission = User.IsInRole(nameof(Role.Admin));

        await bookingService.CancelBookingAsync(id, userId, skipCanselPermission, cancellationToken);

        return NoContent();
    }
}