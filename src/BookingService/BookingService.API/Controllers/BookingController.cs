using BookingService.Application.Interfaces;
using EventOrchestrationService.Contracts.Enums;
using EventOrchestrationService.Contracts.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("bookings")]
[Authorize]
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
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelBooking(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();
        var skipCancelPermission = User.IsInRole(nameof(Role.Admin));

        await bookingService.CancelBookingAsync(id, userId, skipCancelPermission, cancellationToken);

        return NoContent();
    }
}