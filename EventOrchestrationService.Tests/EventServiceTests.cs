using EventOrchestrationService.Models;

namespace EventOrchestrationService.Tests;

public class EventServiceTests
{
    /// <summary>
    /// Создание события с вадидными данными
    /// Проверяем, что событию присваивается новый ИД todo будет переделано с введением dbcontext
    /// Проверяем, что каждое из полей события записано
    /// </summary>
    [Fact]
    public void CreateEvent_WithValidData_Success()
    {
        // Arrange
        var eventService = new EventService();
        const int targetId = 10;

        var validEventToAdd = new Event
        {
            Title = "testTitle1",
            Description = "testDescription1",
            StartAt = new DateTime(2099, 12, 30),
            EndAt = new DateTime(2100, 12, 30)
        };

        // Act
        var result = eventService.CreateEvent(validEventToAdd);

        // Assert
        // todo тест будет переписан когда данные будут храниться в контексте
        Assert.Equal(targetId, result.Id);
        Assert.Equal(validEventToAdd.Title, result.Title);
        Assert.Equal(validEventToAdd.Description, result.Description);
        Assert.Equal(validEventToAdd.StartAt, result.StartAt);
        Assert.Equal(validEventToAdd.EndAt, result.EndAt);

        // todo заглушка из-за отсутствия dbContext'а, удаление только что созданного события чтобы оно не влияло на другие тесты
        eventService.DeleteEvent(targetId);
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
        var eventService = new EventService();
        const int defaultPage = 1;
        const int targetPage = 3;

        // Act
        var getDefaultEventsResult = eventService.GetEvents();
        var getEventsResultWithPageParameter = eventService.GetEvents(page: 3);

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
        var eventService = new EventService();
        const int targetId = 2;

        //todo будет сид данных в фейковый контекст для правильной проверки. Пока берем просто конкретное событие из файла сервиса

        // Act
        var testedEvent = eventService.GetEventById(targetId);

        // Assert
        Assert.Equal(targetId, testedEvent.Id);
    }

    /// <summary>
    /// Обновление события по его id
    /// Проверяем, что при обновлении события каждое его поля корректно перезаписывается
    /// </summary>
    [Fact]
    public void Update_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var eventService = new EventService();
        //todo будет сид данных в фейковый контекст для правильной проверки. Пока берем просто конкретное событие из файла сервиса

        const int targetId = 2;

        var updateRequestData = new Event
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30),
            EndAt = new DateTime(2029, 12, 30)
        };

        // Act
        var updateResult = eventService.UpdateEvent(targetId, updateRequestData);

        // Assert
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
    public void Delete_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var eventService = new EventService();
        const int targetId = 8;

        // Act
        var getEventBeforeDelete = eventService.GetEventById(targetId);
        var deleteResult = eventService.DeleteEvent(targetId);
        var getEventAfterDeleting = eventService.GetEventById(targetId);

        // Assert
        Assert.NotNull(getEventBeforeDelete);
        Assert.True(deleteResult);
        Assert.Null(getEventAfterDeleting);
    }

    /// <summary>
    /// Получить события с фильтром по названию
    /// Проверяем, что получаем в результате все 3 евента у которых в составе названия есть подстрока. Независимо от регистра.
    /// </summary>
    [Fact]
    public void GetEvents_WithTitleFilter_ReturnsMatchingEvents()
    {
        // Arrange
        const int targetResult = 3;
        var expectedIds = new[] { 4, 5, 6 };

        var eventService = new EventService();
        //todo будет сид данных в фейковый контекст для правильной проверки. Пока берем просто конкретные события из файла сервиса

        // Act
        var getResult = eventService.GetEvents(title: "abc");

        // Assert
        Assert.NotNull(getResult);
        Assert.Equal(targetResult, getResult.Items.Count);
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
        var expectedIdsWithFromFilter = new[] { 5, 6, 7 };
        var expectedIdsWithToFilter = new[] { 1, 2, 3, 4, 5, 9 };

        var eventService = new EventService();
        //todo будет сид данных в фейковый контекст для правильной проверки. Пока берем просто конкретные события из файла сервиса

        // Act
        var getResultWithFromFilter = eventService.GetEvents(from: new DateTime(2055, 1, 1));
        var getResultWithToFilter = eventService.GetEvents(to: new DateTime(2030, 12, 31));

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
        var eventService = new EventService();

        const int defaultPage = 1;
        const int targetPage = 3;

        const int defaultPageSize = 10;
        const int targetPageSize = 2;

        var expectedIdsForDefaultParameters = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var expectedIdsForTargetPageSizeParameters = new[] { 1, 2 };
        var expectedIdsForPageAndPageSizeParameter = new[] { 5, 6 };

        // Act
        var getDefaultEventsResult = eventService.GetEvents();

        var getEventsResultWithPageParameter = eventService.GetEvents(page: targetPage);
        var getEventsResultWithPageAndPageSizeParameter =
            eventService.GetEvents(page: targetPage, pageSize: targetPageSize);
        var getEventsResultWithPageSizeParameter = eventService.GetEvents(pageSize: targetPageSize);

        // Assert
        // Проверяем, что по умолчанию получаем по 10 евентов на страницу
        Assert.Equal(defaultPage, getDefaultEventsResult.Page);

        // Проверяем, что на 1 странице всего 9 элементов при дефолтных параметрах пагинации
        // todo вынужденная заглушка из-за отсутствия нормального dbcontext
        Assert.Equal(defaultPageSize - 1, getDefaultEventsResult.PageSize);

        // Проверяем, что для дефолтных параметров пагинации мы получаем все существующие эвенты
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
        var eventService = new EventService();
        var expectedIds = new[] { 4, 5 };

        // Act
        var getResult = eventService.GetEvents(
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
        var eventService = new EventService();

        const int wrongId = 100;

        // Act
        var result = eventService.GetEventById(wrongId);

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
        var eventService = new EventService();

        const int wrongId = 100;

        var updateRequestData = new Event
        {
            Title = "updatedTitle2",
            Description = "updatedDescription2",
            StartAt = new DateTime(2029, 1, 30),
            EndAt = new DateTime(2029, 12, 30)
        };

        // Act
        var result = eventService.UpdateEvent(wrongId, updateRequestData);

        // Assert
        Assert.Null(result);
    }
}