namespace OpenTgResearcherDesktop.PageSelfTests;

// Проверяем настоящие обработчики загрузки при общей активности, вложенных и перекрывающихся действиях
internal static class Program
{
    private sealed class Page : TgPageViewModelBase;

    private static async Task<int> Main()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("PageBusyIsLocal", PageBusyIsLocalAsync),
            ("NestedPageLoading", NestedPageLoadingAsync),
            ("OverlappingPageLoading", OverlappingPageLoadingAsync),
            ("PageFailureReleasesOnlyItsState", PageFailureReleasesOnlyItsStateAsync),
        };
        var failed = 0;
        foreach (var test in tests)
        {
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failed++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failed}/{tests.Length}; Telegram и пользовательская БД не используются");
        return failed == 0 ? 0 : 1;
    }

    // Общая работа парсера не блокирует свободную страницу и сохраняется после локальной загрузки
    private static async Task PageBusyIsLocalAsync()
    {
        using var state = new LoadStateService();
        var parser = Guid.NewGuid(); await state.PrepareStorageTokenAsync(parser);
        state.NetworkActivity.Start(parser, TgNetworkOperationKind.Parsing);
        var page = new Page { LoadStateService = state };
        Assert(!Busy(page), "Чужая операция сделала страницу занятой");
        var ran = false;
        await page.LoadStorageDataAsync(() => { Assert(Busy(page), "Собственная загрузка не отражена"); ran = true; });
        Assert(ran && !Busy(page) && state.IsStorageProcessing && page.Errors.Count == 0, "Локальная загрузка потеряла результат или состояние парсера");
        Assert(state.NetworkActivity.Operations.Single().State == TgNetworkOperationState.Running, "Локальная загрузка завершила парсинг");
        state.StopSoftStorageProcessing(parser);
    }

    // Вложенная загрузка не снимает занятость внешнего обработчика
    private static async Task NestedPageLoadingAsync()
    {
        using var state = new LoadStateService(); var page = new Page { LoadStateService = state }; var nested = false;
        await page.LoadStorageDataAsync(async () =>
        {
            await page.LoadStorageDataAsync(() => { nested = true; });
            Assert(Busy(page), "Внутренний обработчик снял занятость внешнего");
        });
        Assert(nested && !Busy(page) && !state.IsStorageProcessing && page.Errors.Count == 0, "Вложенная загрузка оставила страницу занятой");
    }

    // Более короткая операция не освобождает страницу до завершения более длинной
    private static async Task OverlappingPageLoadingAsync()
    {
        using var state = new LoadStateService(); var page = new Page { LoadStateService = state };
        var entered = new TaskCompletionSource(); var release = new TaskCompletionSource(); var completed = false;
        var first = page.LoadStorageDataAsync(async () => { entered.SetResult(); await release.Task; completed = true; });
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            await page.LoadStorageDataAsync(() => { });
            Assert(Busy(page) && !completed, "Короткая операция преждевременно освободила страницу");
        }
        finally { release.TrySetResult(); await first; }
        Assert(completed && !Busy(page) && !state.IsStorageProcessing, "Последняя загрузка не освободила страницу");
    }

    // Ошибка регистрируется, а освобождение страницы не сбрасывает чужой токен
    private static async Task PageFailureReleasesOnlyItsStateAsync()
    {
        using var state = new LoadStateService(); var parser = Guid.NewGuid(); await state.PrepareStorageTokenAsync(parser);
        var page = new Page { LoadStateService = state };
        await page.LoadStorageDataAsync((Action)(() => throw new InvalidOperationException("Ошибка чтения тестовой страницы")));
        Assert(!Busy(page) && page.Errors.Single().Message == "Ошибка чтения тестовой страницы" && state.IsStorageProcessing,
            "Ошибка скрыта или сбросила состояние чужой операции");
        state.StopSoftStorageProcessing(parser);
    }

    // Проверяем свойство исходной модели до и после реализации без дублирования её счётчика
    private static bool Busy(Page page)
    {
        var property = typeof(Page).GetProperty("IsPageStorageProcessing")
            ?? throw new InvalidOperationException("Нет отдельного состояния загрузки страницы");
        return (bool)property.GetValue(page)!;
    }

    // Не допускаем успешного результата при отсутствии требуемых действий
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
