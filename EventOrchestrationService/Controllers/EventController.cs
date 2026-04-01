using EventOrchestrationService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventOrchestrationService.Controllers;

[ApiController]
[Route("events")]
public class EventController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получить список всех событий
    /// </summary>
    /// <returns>Список всех событий</returns>
    [HttpGet]
    public IActionResult GetAllEvents()
    {
        return Ok(eventService.GetAllEvents());
    }

    /// <summary>
    /// Получить событие по ID
    /// </summary>
    /// <param name="id">ID события</param>
    /// <returns>Событие с указанным ID</returns>
    [HttpGet("{id:int}")]
    public IActionResult GetEventById(int id)
    {
        var targetEvent = eventService.GetEventById(id);

        if (targetEvent == null)
        {
            return NotFound($"Событие с ID {id} не найдено");
        }

        return Ok(targetEvent);
    }

    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="newEvent">Данные события</param>
    /// <returns>Созданное событие</returns>
    [HttpPost]
    public IActionResult Create([FromBody] Event newEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdEvent = eventService.CreateEvent(newEvent);

        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    /// <summary>
    /// Обновить существующее событие
    /// </summary>
    /// <param name="id">ID события</param>
    /// <param name="updateEventRequest">Новые данные события</param>
    /// <returns>Обновлённое событие</returns>
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Event updateEventRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedEventResult = eventService.UpdateEvent(id, updateEventRequest);

        if (updatedEventResult == null)
        {
            return NotFound($"Событие с ID {id} не найдено");
        }

        return Ok(updatedEventResult);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">ID события</param>
    /// <returns>Статус удаления</returns>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleteResult = eventService.DeleteEvent(id);

        return deleteResult ? NoContent() : NotFound($"Событие с ID {id} не найдено");
    }
}