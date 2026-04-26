namespace EventOrchestrationService.Models.DTO;

public class PaginatedResult
{
    /// <summary>
    /// Общее количество событий
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Массив самих событий
    /// </summary>
    public List<Event> Items { get; set; }

    /// <summary>
    /// Номер текущей страницы
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Количество элементов на текущей странице
    /// </summary>
    public int PageSize { get; set; }
}