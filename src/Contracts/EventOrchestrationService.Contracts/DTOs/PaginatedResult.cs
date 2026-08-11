namespace EventOrchestrationService.Contracts.DTOs;

public class PaginatedResult<T>
{
    /// <summary>
    /// Общее количество событий
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Массив объектов пагинации
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Номер текущей страницы
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Количество элементов на текущей странице
    /// </summary>
    public int PageSize { get; set; }
}