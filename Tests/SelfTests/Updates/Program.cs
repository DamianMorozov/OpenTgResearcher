namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Проверяем исходную VM и навигацию с настоящими строками обоих языков
    private static async Task<int> Main(string[] args)
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("SettingsRussian", () => SettingsLocalizedAsync("ru-RU", "Настройки")),
            ("SettingsEnglish", () => SettingsLocalizedAsync("en-US", "Settings")),
            ("SettingsNavigation", SettingsNavigationAsync),
            ("SettingsLanguageChange", SettingsLanguageChangeAsync),
            ("UpdateRussian", () => UpdateLocalizedAsync("ru-RU", "Установлена последняя версия")),
            ("UpdateEnglish", () => UpdateLocalizedAsync("en-US", "latest version")),
            ("AllUpdateStringsLocalized", AllUpdateStringsLocalizedAsync),
            ("RejectUpdate", RejectUpdateAsync),
            ("MissingWindow", MissingWindowAsync),
            ("WaitForCheck", WaitForCheckAsync),
            ("StartAfterDialogClosed", StartAfterDialogClosedAsync),
            ("ExcludeParallelUpdates", ExcludeParallelUpdatesAsync),
            ("UninstalledGuidance", UninstalledGuidanceAsync),
            ("DownloadCompletionAndProgress", DownloadCompletionAndProgressAsync),
            ("DownloadCancellation", DownloadCancellationAsync),
            ("CancelButton", CancelButtonAsync),
            ("CancelDuringCheck", CancelDuringCheckAsync),
            ("ErrorGuidanceAndLog", ErrorGuidanceAndLogAsync),
            ("DownloadErrorDoesNotReportSuccess", DownloadErrorDoesNotReportSuccessAsync),
            ("PreviewSource", PreviewSourceAsync),
            ("NavigationIndependentOfTelegram", NavigationIndependentOfTelegramAsync),
            ("ReloadKeepsResult", ReloadKeepsResultAsync),
            ("UpdateLogLayout", UpdateLogLayoutAsync),
            ("ToolbarLanguageChoice", ToolbarLanguageChoiceAsync),
            ("ToolbarThemeChoice", ToolbarThemeChoiceAsync),
            ("ToolbarRejectsInvalidChoice", ToolbarRejectsInvalidChoiceAsync),
            ("ToolbarWaitsForSharedUpdate", ToolbarWaitsForSharedUpdateAsync),
            ("ToolbarCancelConfirmation", ToolbarCancelConfirmationAsync),
            ("ToolbarControlsLayout", ToolbarControlsLayoutAsync),
            ("ToolbarSensitiveDataLayout", ToolbarSensitiveDataLayoutAsync),
            ("ToolbarStringsRussian", () => ToolbarStringsAsync("ru-RU", "Язык", "Тема", "Проверить обновления")),
            ("ToolbarStringsEnglish", () => ToolbarStringsAsync("en-US", "Language", "Theme", "Check for updates")),
        };
        var failed = 0;
        foreach (var test in tests.Where(test => args.Length < 2 || args[0] != "--test" || args[1] == test.Name))
        {
            UpdateManager.State = new();
            ContentDialog.Confirm = true;
            ContentDialog.Gate = null;
            Localizer.Language = "ru-RU";
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failed++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
            finally
            {
                ContentDialog.Gate?.TrySetResult();
                UpdateManager.State.CheckGate?.TrySetResult();
                UpdateManager.State.DownloadGate?.TrySetResult();
                if (ContentDialog.LastAction is not null) await ContentDialog.LastAction;
            }
        }
        Console.WriteLine($"Ошибок: {failed}");
        return failed == 0 ? 0 : 1;
    }

    // Создаём исходную модель без БД пользователя и обращения к Telegram
    private static TgUpdateViewModel CreateViewModel() => new(new(), new(), new(), null!);

    // Проверяем конкретную подпись встроенного пункта и подключение ресурса
    private static Task SettingsLocalizedAsync(string language, string expected)
    {
        Localizer.Language = language;
        var service = new NavigationViewService(new(), new());
        var view = new NavigationView();
        service.Initialize(view);
        view.RaiseLoaded();
        var item = (NavigationViewItem)view.SettingsItem;
        Assert(item.Uid == "Shell_Settings", "Встроенный пункт настроек не подключён к ресурсам приложения");
        Assert(item.Content == expected, $"Неверная подпись: {item.Content}");
        Assert(item.AutomationName == expected, "Имя встроенной кнопки настроек не переведено");
        service.UnregisterEvents();
        return Task.CompletedTask;
    }

    // Переключение языка обновляет и видимую подпись, и имя для доступности без перезапуска
    private static Task SettingsLanguageChangeAsync()
    {
        var service = new NavigationViewService(new(), new());
        var view = new NavigationView();
        service.Initialize(view);
        view.RaiseLoaded();
        try
        {
            Localizer.Language = "en-US";
            Assert(((NavigationViewItem)view.SettingsItem).Content == "Settings", "Подпись настроек не переключилась на английский");
            Localizer.Language = "ru-RU";
            var item = (NavigationViewItem)view.SettingsItem;
            Assert(item.Content == "Настройки" && item.AutomationName == "Настройки", "Подпись настроек не вернулась на русский");
        }
        finally { service.UnregisterEvents(); }
        return Task.CompletedTask;
    }

    // Локализация сохраняет штатную навигацию на настройки
    private static Task SettingsNavigationAsync()
    {
        var navigation = new INavigationService();
        var service = new NavigationViewService(navigation, new());
        var view = new NavigationView();
        service.Initialize(view);
        view.RaiseLoaded();
        view.InvokeSettings();
        Assert(navigation.LastPage == typeof(TgSettingsViewModel).FullName, "Выбор настроек не открыл нужную страницу");
        service.UnregisterEvents();
        return Task.CompletedTask;
    }

    // Динамический журнал использует выбранный язык ресурсов
    private static async Task UpdateLocalizedAsync(string language, string expected)
    {
        Localizer.Language = language;
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(vm.UpdateLog.Contains(expected, StringComparison.OrdinalIgnoreCase), "Результат проверки не переведён: " + vm.UpdateLog);
        Assert(!vm.UpdateLog.Contains("Update started", StringComparison.Ordinal), "В журнале осталась фиксированная английская строка");
        Assert(UpdateManager.State.Checks == 1 && UpdateManager.State.Downloads == 0, "Не выполнена проверка без обновления");
    }

    // Отказ пользователя не вызывает сеть
    private static async Task RejectUpdateAsync()
    {
        ContentDialog.Confirm = false;
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(UpdateManager.State.Created == 0 && vm.UpdateLog.Length == 0, "Отменённый диалог начал проверку");
    }

    // Отсутствующее окно не запускает обновление
    private static async Task MissingWindowAsync()
    {
        var vm = CreateViewModel();
        vm.XamlRootVm = null;
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(UpdateManager.State.Created == 0, "Проверка началась без подтверждения");
    }

    // Команда остаётся выполняющейся до получения результата источника
    private static async Task WaitForCheckAsync()
    {
        UpdateManager.State.CheckGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdateReleaseCommand.ExecuteAsync(null);
        await WaitAsync(() => UpdateManager.State.Checks == 1);
        var completedTooEarly = action.IsCompleted;
        UpdateManager.State.CheckGate.SetResult();
        await action;
        Assert(!completedTooEarly, "Команда завершилась до окончания проверки обновлений");
    }

    // Сеть вызывается только после закрытия подтверждения
    private static async Task StartAfterDialogClosedAsync()
    {
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(UpdateManager.State.Checks == 1 && !UpdateManager.State.StartedInsideDialog, "Проверка запущена внутри открытого диалога");
    }

    // Обе команды блокируются уже на этапе подтверждения
    private static async Task ExcludeParallelUpdatesAsync()
    {
        ContentDialog.Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdateReleaseCommand.ExecuteAsync(null);
        try
        {
            Assert(!vm.UpdateReleaseCommand.CanExecute(null) && !vm.UpdatePreviewCommand.CanExecute(null), "Можно одновременно запустить два обновления");
        }
        finally { ContentDialog.Gate.SetResult(); await action; }
        Assert(vm.UpdateReleaseCommand.CanExecute(null) && vm.UpdatePreviewCommand.CanExecute(null), "Команды не разблокированы после операции");
    }

    // Запуск из каталога публикации объясняется до сетевого запроса
    private static async Task UninstalledGuidanceAsync()
    {
        UpdateManager.State.Installed = false;
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(UpdateManager.State.Checks == 0 && vm.UpdateLog.Contains("Setup.exe"), "Нет инструкции установки для сборки без Velopack");
    }

    // Прогресс показывается до завершения, результат содержит версию и дальнейшее действие
    private static async Task DownloadCompletionAndProgressAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.DownloadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdateReleaseCommand.ExecuteAsync(null);
        await WaitAsync(() => UpdateManager.State.Downloads == 1);
        var progress = Read<int>(vm, "DownloadProgress");
        var waiting = !action.IsCompleted;
        UpdateManager.State.DownloadGate.SetResult();
        await action;
        Assert(waiting && progress == 37, "Прогресс не поступает во время загрузки");
        Assert(Read<int>(vm, "DownloadProgress") == 100 && !Read<bool>(vm, "IsUpdating"), "Загрузка не завершена в модели");
        Assert(vm.UpdateLog.Contains("0.8.81") && vm.UpdateLog.Contains("перезапустите", StringComparison.OrdinalIgnoreCase), "Нет результата и инструкции перезапуска");
    }

    // Отмена проходит до загрузчика и позволяет повторить команду
    private static async Task DownloadCancellationAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.DownloadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdateReleaseCommand.ExecuteAsync(null);
        await WaitAsync(() => UpdateManager.State.Downloads == 1);
        var cancellable = vm.UpdateReleaseCommand.CanBeCanceled;
        vm.UpdateReleaseCommand.Cancel();
        if (!cancellable) UpdateManager.State.DownloadGate.SetResult();
        await action;
        Assert(cancellable && UpdateManager.State.DownloadCancelled, "Отмена команды не передана загрузчику");
        Assert(vm.UpdateLog.Contains("отменена") && vm.Errors.Count == 0, "Отмена не объяснена или записана как ошибка");
        Assert(vm.UpdatePreviewCommand.CanExecute(null), "После отмены нельзя повторить проверку");
    }

    // Кнопка отмены вызывает активную команду и останавливает передачу пакета
    private static async Task CancelButtonAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.DownloadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdatePreviewCommand.ExecuteAsync(null);
        await WaitAsync(() => UpdateManager.State.Downloads == 1);
        var cancel = Read<IRelayCommand>(vm, "CancelUpdateCommand");
        Assert(cancel.CanExecute(null), "Кнопка отмены недоступна во время загрузки");
        cancel.Execute(null);
        await action.WaitAsync(TimeSpan.FromSeconds(3));
        Assert(UpdateManager.State.DownloadCancelled && !cancel.CanExecute(null), "Кнопка не отменила загрузку или осталась активной");
    }

    // Velopack заканчивает уже начатую проверку, после отмены пакет не скачивается
    private static async Task CancelDuringCheckAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.CheckGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateViewModel();
        var action = vm.UpdateReleaseCommand.ExecuteAsync(null);
        await WaitAsync(() => UpdateManager.State.Checks == 1);
        vm.UpdateReleaseCommand.Cancel();
        UpdateManager.State.CheckGate.SetResult();
        await action;
        Assert(UpdateManager.State.Downloads == 0 && vm.UpdateLog.Contains("отменена"), "После отмены проверки началась загрузка");
    }

    // Все сообщения и подписи страницы существуют на обоих языках и отличаются переводом
    private static Task AllUpdateStringsLocalizedAsync()
    {
        var keys = new[] { "Shell_Settings", "ActionUpdateReleaseApp", "ActionUpdatePreviewApp", "ActionCancelUpdate", "UpdateLogCaption",
            "UpdateStarted", "UpdateCheckingRelease", "UpdateCheckingPreview", "UpdateAlreadyLatest", "UpdateNotInstalled", "UpdateDownloading",
            "UpdateDownloaded", "UpdateCancelled", "UpdateFailed" };
        foreach (var key in keys)
        {
            Localizer.Language = "ru-RU";
            var russian = key.GetLocalizedResource();
            Localizer.Language = "en-US";
            var english = key.GetLocalizedResource();
            Assert(!string.IsNullOrWhiteSpace(russian) && !string.IsNullOrWhiteSpace(english) && russian != english, $"Не переведён ресурс: {key}");
            _ = string.Format(CultureInfo.InvariantCulture, russian, "1.2.3");
            _ = string.Format(CultureInfo.InvariantCulture, english, "1.2.3");
        }
        return Task.CompletedTask;
    }

    // Повторное открытие и обновление страницы сохраняют результат и не вызывают Telegram
    private static async Task ReloadKeepsResultAsync()
    {
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        var original = vm.UpdateLog;
        await vm.ReloadUiAsync();
        await vm.OnNavigatedToAsync(null);
        Assert(original.Length > 0 && vm.UpdateLog == original && vm.TelegramChecks == 0, "Обновление страницы потеряло результат или проверило Telegram");
    }

    // Ошибка объясняется на языке интерфейса, оригинал остаётся в журнале
    private static async Task ErrorGuidanceAndLogAsync()
    {
        var error = new IOException("test network failure");
        UpdateManager.State.CheckError = error;
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(vm.Errors.Single() == error, "В журнале потеряно исходное исключение");
        Assert(vm.UpdateLog.Contains("Не удалось") && vm.UpdateLog.Contains(error.Message), "Нет переведённого объяснения ошибки");
        Assert(vm.UpdateReleaseCommand.CanExecute(null), "Ошибка оставила команду заблокированной");
    }

    // Неудачная загрузка не объявляется успешной
    private static async Task DownloadErrorDoesNotReportSuccessAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.DownloadError = new IOException("test download failure");
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdateReleaseCommand);
        Assert(UpdateManager.State.Downloads == 1 && vm.Errors.Count == 1, "Ошибка загрузки потеряна");
        Assert(!vm.UpdateLog.Contains("перезапустите", StringComparison.OrdinalIgnoreCase), "Пользователю показано ложное завершение");
    }

    // Предварительная версия использует соответствующий источник GitHub
    private static async Task PreviewSourceAsync()
    {
        var vm = CreateViewModel();
        await ExecuteAsync(vm.UpdatePreviewCommand);
        Assert(UpdateManager.State.Checks == 1 && UpdateManager.State.Preview, "Проверен источник стабильных версий вместо предварительных");
    }

    // Просмотр обновлений не проверяет Telegram и не использует БД
    private static async Task NavigationIndependentOfTelegramAsync()
    {
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        Assert(vm.StorageCalls == 0 && vm.TelegramChecks == 0, "Открытие страницы зависит от хранилища или Telegram");
    }

    // Длинный журнал можно читать и копировать в ограниченном окне
    private static Task UpdateLogLayoutAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("TgUpdatePage.xaml")!;
        var xaml = XDocument.Load(stream);
        var log = xaml.Descendants().Single(item => ((string?)item.Attribute("Text"))?.Contains("ViewModel.UpdateLog") == true);
        Assert(log.Name.LocalName == "TextBox" && (string?)log.Attribute("IsReadOnly") == "True", "Журнал нельзя выделить и скопировать");
        Assert((string?)log.Attribute("ScrollViewer.VerticalScrollBarVisibility") == "Auto", "Длинный журнал не прокручивается");
        Assert((string?)log.Attribute("TextWrapping") == "Wrap", "Длинная строка журнала обрезается");
        return Task.CompletedTask;
    }

    // Ожидаем и старую отделённую команду, чтобы сравнить конечное наблюдаемое поведение
    private static async Task ExecuteAsync(IAsyncRelayCommand command)
    {
        await command.ExecuteAsync(null);
        if (ContentDialog.LastAction is not null) await ContentDialog.LastAction;
    }

    // Читаем наблюдаемые свойства без требования их наличия до исправления
    private static T Read<T>(object instance, string name)
    {
        var property = instance.GetType().GetProperty(name);
        Assert(property is not null, $"В модели отсутствует {name}");
        return (T)property!.GetValue(instance)!;
    }

    // Ограничиваем ожидание запуска подменённой сетевой операции
    private static async Task WaitAsync(Func<bool> predicate)
    {
        var timeout = DateTime.UtcNow.AddSeconds(3);
        while (!predicate() && DateTime.UtcNow < timeout) await Task.Delay(5);
        Assert(predicate(), "Команда не выполнила ожидаемую операцию");
    }

    // Завершаем проверку при отсутствии наблюдаемого результата
    private static void Assert(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
