using EventOrchestrationService.Exceptions;
using EventOrchestrationService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventOrchestrationService.Controllers;

[ApiController]
[Route("events")]
public class EventController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий.
    /// </summary>
    /// <param name="title">Опциональный, получить события по полю title. Регистронезависимый, частичное совпадение.</param>
    /// <param name="from">Опциональный, события, которые начинаются не раньше указанной даты.</param>
    /// <param name="to">Опциональный, события, которые заканчиваются не позже указанной даты.</param>
    /// <param name="page">Опциональный (по умолчанию = 1), страница, которую необходимо вернуть.</param>
    /// <param name="pageSize">Опциональный (по умолчанию = 10), количество элементов на странице.</param>
    /// <returns>
    /// Объект PaginatedResult содержащий:
    /// - TotalCount: общее количество отфильтрованных событий
    /// - Items: список событий на текущей странице
    /// - Page: текущая страница
    /// - PageSize: фактическое количество элементов на странице
    /// </returns>
    /// <response code="200">Успешный возврат пагинированного списка</response>
    [HttpGet]
    public IActionResult GetEvents(string? title, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        return Ok(eventService.GetEvents(title, from, to, page, pageSize));
    }

    /// <summary>
    /// Получить событие по ID.
    /// </summary>
    /// <param name="id">ID события.</param>
    /// <returns>Событие с указанным ID.</returns>
    [HttpGet("{id:int}")]
    public IActionResult GetEventById(int id)
    {
        var targetEvent = eventService.GetEventById(id);

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
    /// <returns>Созданное событие.</returns>
    [HttpPost]
    public IActionResult Create([FromBody] Event newEvent)
    {
        var createdEvent = eventService.CreateEvent(newEvent);
        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    /// <summary>
    /// Обновить существующее событие.
    /// </summary>
    /// <param name="id">ID события.</param>
    /// <param name="updateEventRequest">Новые данные события.</param>
    /// <returns>Обновлённое событие.</returns>
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Event updateEventRequest)
    {
        var updatedEventResult = eventService.UpdateEvent(id, updateEventRequest);

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
    /// <returns>Статус удаления.</returns>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleteResult = eventService.DeleteEvent(id);

        if (!deleteResult)
        {
            throw new NotFoundException($"Событие с ID {id} не найдено");
        }

        return NoContent();
    }
}