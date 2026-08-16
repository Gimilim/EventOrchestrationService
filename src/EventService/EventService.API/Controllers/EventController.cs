using EventOrchestrationService.Contracts.Enums;
using EventOrchestrationService.Contracts.Exceptions;
using EventOrchestrationService.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.API.Controllers;

[ApiController]
[Route("events")]
[Authorize]
public class EventController(IEventService eventService, IBookingService bookingService)
    : ApiControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    /// <param name="title">Опциональный, получить события по полю title. Регистронезависимый, частичное совпадение.</param>
    /// <param name="from">Опциональный, события, которые начинаются не раньше указанной даты.</param>
    /// <param name="to">Опциональный, события, которые заканчиваются не позже указанной даты.</param>
    /// <param name="page">Опциональный (по умолчанию = 1), страница, которую необходимо вернуть.</param>
    /// <param name="pageSize">Опциональный (по умолчанию = 10), количество элементов на странице.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>
    /// Объект PaginatedResult содержащий:
    /// - TotalCount: общее количество отфильтрованных событий
    /// - Items: список событий на текущей странице
    /// - Page: текущая страница
    /// - PageSize: фактическое количество элементов на странице
    /// </returns>
    /// <response code="200">Успешный возврат пагинированного списка</response>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents(string? title, DateTime? from, DateTime? to, int page = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return Ok(await eventService.GetEventsAsync(title, from, to, page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Получить событие по ID.
    /// </summary>
    /// <param name="id">ID события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Событие с указанным ID.</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventById(int id, CancellationToken cancellationToken)
    {
        var targetEvent = await eventService.GetEventByIdAsync(id, cancellationToken);

        if (targetEvent == null)
        {
            throw new NotFoundException($"Событие с ID {id} не найдено");
        }

        return Ok(targetEvent);
    }

    /// <summary>
    /// Создать новое событие.
    /// </summary>
    /// <param name="newEvent">Данные события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданное событие.</returns>
    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] CreateEventDto newEvent, CancellationToken cancellationToken)
    {
        var createdEvent = await eventService.CreateEventAsync(newEvent, cancellationToken);
        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    /// <summary>
    /// Обновить существующее событие.
    /// </summary>
    /// <param name="id">ID события.</param>
    /// <param name="updateEventRequest">Новые данные события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновлённое событие.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto updateEventRequest,
        CancellationToken cancellationToken)
    {
        var updatedEventResult = await eventService.UpdateEventAsync(id, updateEventRequest, cancellationToken);

        if (updatedEventResult == null)
        {
            throw new NotFoundException($"Событие с ID {id} не найдено");
        }

        return Ok(updatedEventResult);
    }

    /// <summary>
    /// Удалить событие.
    /// </summary>
    /// <param name="id">ID события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Статус удаления.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleteResult = await eventService.DeleteEventAsync(id, cancellationToken);

        if (!deleteResult)
        {
            throw new NotFoundException($"Событие с ID {id} не найдено");
        }

        return NoContent();
    }
}