using EventOrchestrationService.Data;
using EventOrchestrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventOrchestrationService.Tests;

public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IEventService _service;

    public EventServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _service = new EventService(_context, new Event.EventValidator());
    }

    public void Dispose()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
    }

    private void SeedDatabase()
    {
        _context.Events.AddRange(
            new Event
            {
                Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.Now.AddDays(-5),
                EndAt = DateTime.Now.AddDays(3)
            },
            new Event
            {
                Id = 2, Title = "Title2", Description = "Description2", StartAt = new DateTime(2025, 1, 30),
                EndAt = new DateTime(2025, 12, 30)
            },
            new Event
            {
                Id = 3, Title = "Title3", Description = "Description3", StartAt = DateTime.Now.AddDays(-8),
                EndAt = DateTime.Now.AddDays(5)
            },
            new Event
            {
                Id = 4, Title = "ABC_Title4", Description = "Description4", StartAt = DateTime.Now.AddDays(-8),
                EndAt = DateTime.Now.AddDays(5)
            },
            new Event
            {
                Id = 5, Title = "abc_Title5", Description = "Description5", StartAt = new DateTime(2055, 1, 30),
                EndAt = DateTime.Now.AddDays(5)
            },
            new Event
            {
                Id = 6, Title = "AbC_Title6", Description = "Description6", StartAt = new DateTime(2055, 1, 30),
                EndAt = new DateTime(2077, 12, 30)
            },
            new Event
            {
                Id = 7, Title = "Title7", Description = "Description7", StartAt = new DateTime(2055, 1, 30),
                EndAt = new DateTime(2077, 12, 30)
            },
            new Event
            {
                Id = 8, Title = "Title8", Description = "Description8", StartAt = new DateTime(2025, 1, 30),
                EndAt = new DateTime(2077, 12, 30)
            },
            new Event
            {
                Id = 9, Title = "Title9", Description = "Description9", StartAt = new DateTime(2027, 1, 30),
                EndAt = new DateTime(2027, 12, 30)
            }
        );
        _context.SaveChanges();
    }

    /// <summary>
    /// Создание события с валидными данными
    /// Проверяем, что событию присваивается новый ИД todo будет переделано с введением dbcontext
    /// Проверяем, что каждое из полей события записано
    /// </summary>
    [Fact]
    public void CreateEvent_WithValidData_Success()
    {
        // Arrange
        var validEventToAdd = new Event
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30),
            EndAt = new DateTime(2100, 12, 30)
        };

        // Act
        var result = _service.CreateEvent(validEventToAdd);

        // Assert
        Assert.Equal(validEventToAdd.Title, result.Title);
        Assert.Equal(validEventToAdd.Description, result.Description);
        Assert.Equal(validEventToAdd.StartAt, result.StartAt);
        Assert.Equal(validEventToAdd.EndAt, result.EndAt);
    }

    /// <summary>
    /// Получение всех событий
    /// Проверяем, что результат запроса не пустой (получили объект пагинации)
    /// Проверяем, что по дефолту, без параметров, возвращается первая страница
    /// Проверяем, что можем запросить конкретную страницу и получить ожидаемый результат
    /// </summary>
    [Fact]
    public void GetEvents_WhenEventsExist_ReturnsPaginatedResult()
    {
        // Arrange
        SeedDatabase();

        const int defaultPage = 1;
        const int targetPage = 3;

        // Act
        var getDefaultEventsResult = _service.GetEvents();
        var getEventsResultWithPageParameter = _service.GetEvents(page: 3);

        // Assert
        Assert.NotNull(getDefaultEventsResult);
        Assert.NotNull(getEventsResultWithPageParameter);

        Assert.Equal(defaultPage, getDefaultEventsResult.Page);
        Assert.Equal(targetPage, getEventsResultWithPageParameter.Page);
    }

    /// <summary>
    /// Получение события по id
    /// Проверяем, что при запросе конкретного собятия получаем именно его
    /// </summary>
    [Fact]
    public void GetEventById_WithValidId_ReturnsEvent()
    {
        // Arrange
        var validEventToAdd = new Event
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30),
            EndAt = new DateTime(2100, 12, 30)
        };

        var created = _service.CreateEvent(validEventToAdd);

        // Act
        var result = _service.GetEventById(validEventToAdd.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Title, result.Title);
        Assert.Equal(created.Description, result.Description);
        Assert.Equal(created.StartAt, result.StartAt);
        Assert.Equal(created.EndAt, result.EndAt);
    }

    /// <summary>
    /// Обновление события по его id
    /// Проверяем, что при обновлении события каждое его поля корректно перезаписывается
    /// </summary>
    [Fact]
    public void Update_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var validEventToAdd = new Event
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30),
            EndAt = new DateTime(2100, 12, 30)
        };

        var created = _service.CreateEvent(validEventToAdd);

        var updateRequestData = new Event
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30),
            EndAt = new DateTime(2029, 12, 30)
        };

        // Act
        var updateResult = _service.UpdateEvent(created.Id, updateRequestData);

        // Assert
        Assert.NotNull(updateResult);
        Assert.Equal(updateRequestData.Title, updateResult.Title);
        Assert.Equal(updateRequestData.Description, updateResult.Description);
        Assert.Equal(updateRequestData.StartAt, updateResult.StartAt);
        Assert.Equal(updateRequestData.EndAt, updateResult.EndAt);
    }

    /// <summary>
    /// Удаление события по его ИД
    /// Проверяем, что до удаления событие существует до удаления
    /// Проверяем, что событие успешно удаляется
    /// </summary>
    [Fact]
    public void Delete_WithValidId_DeleteSuccess()
    {
        var validEventToAdd = new Event
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30),
            EndAt = new DateTime(2100, 12, 30)
        };

        var created = _service.CreateEvent(validEventToAdd);

        // Act
        var getEventBeforeDelete = _service.GetEventById(created.Id);
        var deleteResult = _service.DeleteEvent(created.Id);
        var getEventAfterDeleting = _service.GetEventById(created.Id);

        // Assert
        Assert.NotNull(getEventBeforeDelete);
        Assert.True(deleteResult);
        Assert.Null(getEventAfterDeleting);
    }

    /// <summary>
    /// Получить события с фильтром по названию
    /// Проверяем, что получаем в результате все 3 события у которых в составе названия есть подстрока. Независимо от регистра.
    /// </summary>
    [Fact]
    public void GetEvents_WithTitleFilter_ReturnsMatchingEvents()
    {
        // Arrange
        SeedDatabase();
        var expectedIds = new[] { 4, 5, 6 };

        // Act
        var getResult = _service.GetEvents(title: "abc");

        // Assert
        Assert.NotNull(getResult);
        Assert.Equal(expectedIds.Length, getResult.Items.Count);
        Assert.Equal(expectedIds, getResult.Items.Select(e => e.Id));
    }

    /// <summary>
    /// Получить события с фильтром по дате (startDate, endDate)
    /// Проверяем, что получаем правильный результат с фильтрами по дате
    /// </summary>
    [Fact]
    public void GetEvents_WithDateRange_ReturnsEventsWithinRange()
    {
        // Arrange
        SeedDatabase();
        var expectedIdsWithFromFilter = new[] { 5, 6, 7 };
        var expectedIdsWithToFilter = new[] { 1, 2, 3, 4, 5, 9 };

        // Act
        var getResultWithFromFilter = _service.GetEvents(from: new DateTime(2055, 1, 1));
        var getResultWithToFilter = _service.GetEvents(to: new DateTime(2030, 12, 31));

        // Assert
        Assert.Equal(expectedIdsWithFromFilter, getResultWithFromFilter.Items.Select(e => e.Id));
        Assert.Equal(expectedIdsWithToFilter, getResultWithToFilter.Items.Select(e => e.Id));
    }

    /// <summary>
    /// Получить события с пагинацией
    /// Проверяем, что получаем ожидаемый набор события для разных настроей пагинации
    /// </summary>
    [Fact]
    public void GetEvents_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        SeedDatabase();
        const int defaultPage = 1;
        const int targetPage = 3;

        const int defaultPageSize = 10;
        const int targetPageSize = 2;

        var expectedIdsForDefaultParameters = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var expectedIdsForTargetPageSizeParameters = new[] { 1, 2 };
        var expectedIdsForPageAndPageSizeParameter = new[] { 5, 6 };

        // Act
        var getDefaultEventsResult = _service.GetEvents();

        var getEventsResultWithPageParameter = _service.GetEvents(page: targetPage);
        var getEventsResultWithPageAndPageSizeParameter =
            _service.GetEvents(page: targetPage, pageSize: targetPageSize);
        var getEventsResultWithPageSizeParameter = _service.GetEvents(pageSize: targetPageSize);

        // Assert
        // Проверяем, что по умолчанию получаем по 10 событий на страницу
        Assert.Equal(defaultPage, getDefaultEventsResult.Page);

        // Проверяем, что на 1 странице всего 9 элементов при дефолтных параметрах пагинации
        Assert.Equal(9, getDefaultEventsResult.PageSize);

        // Проверяем, что для дефолтных параметров пагинации мы получаем все существующие события
        Assert.Equal(expectedIdsForDefaultParameters, getDefaultEventsResult.Items.Select(e => e.Id));

        // Проверяем, что если явно задаем страницу -- получаем ее
        Assert.Equal(targetPage, getEventsResultWithPageParameter.Page);

        // Проверяем, что явно указав только количество событий на странице -- получаем их же (при условии, что страница полностью заполнена)
        Assert.Equal(targetPageSize, getEventsResultWithPageSizeParameter.PageSize);

        // Проверяем, что для конкретной страницы получаем те события, которые ожидаем на ней увидеть
        Assert.Equal(expectedIdsForPageAndPageSizeParameter,
            getEventsResultWithPageAndPageSizeParameter.Items.Select(e => e.Id));

        // Проверяем, что если задаем размер страница получаем те события, которые должны на дефолтной странице
        Assert.Equal(expectedIdsForTargetPageSizeParameters,
            getEventsResultWithPageSizeParameter.Items.Select(e => e.Id));
    }

    /// <summary>
    /// Получить события с комбинированной фильтрацией
    /// Проверяем, что получаем ожидаемый набор события для комбинации фильтров
    /// </summary>
    [Fact]
    public void GetEvents_WithMultipleFilters_ReturnsFilteredEvents()
    {
        // Arrange
        SeedDatabase();
        var expectedIds = new[] { 4, 5 };

        // Act
        var getResult = _service.GetEvents(
            title: "abc",
            from: new DateTime(2025, 1, 1),
            to: new DateTime(2029, 1, 1)
        );

        // Assert
        Assert.Equal(expectedIds, getResult.Items.Select(e => e.Id));
    }

    /// <summary>
    /// Попытка получить событие с несуществующим ID
    /// Проверяем, что получаем null в ответ
    /// </summary>
    [Fact]
    public void GetEventById_WithNonExistentId_ReturnNull()
    {
        // Arrange
        const int wrongId = 100;

        // Act
        var result = _service.GetEventById(wrongId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Попытка обновить событие с несуществующим ID
    /// Проверяем, что получаем null в ответ
    /// </summary>
    [Fact]
    public void Update_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        const int wrongId = 100;

        var updateRequestData = new Event
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30),
            EndAt = new DateTime(2029, 12, 30)
        };

        // Act
        var result = _service.UpdateEvent(wrongId, updateRequestData);

        // Assert
        Assert.Null(result);
    }
}