namespace OpenTgResearcherDesktop.ConnectionSelfTests;

internal static partial class Program
{
    // Проверяем исходную модель страницы с репозиториями в памяти и отключёнными сетевыми службами
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--inspect-session") return await InspectSessionAsync(args);
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("SignInWizardIntegration", SignInWizardIntegrationAsync),
            ("SignInExistingSessionStartsCenter", SignInExistingSessionStartsCenterAsync),
            ("SignInDisconnectResetsRequest", SignInDisconnectResetsRequestAsync),
            ("SignInPageResources", SignInPageResourcesAsync),
            ("ConstructorInitializesModel", ConstructorInitializesModelAsync),
            ("ConnectionReadsWhileParserBusy", ConnectionReadsWhileParserBusyAsync),
            ("ApplyProxyFromNavigation", ApplyProxyFromNavigationAsync),
            ("ApplyProxyToNewProfile", ApplyProxyToNewProfileAsync),
            ("ApplyMissingProxy", ApplyMissingProxyAsync),
            ("ApplyProxySaveFailure", ApplyProxySaveFailureAsync),
            ("NavigationSelectsSavedProxy", NavigationSelectsSavedProxyAsync),
            ("SavePreservesSelectedProxy", SavePreservesSelectedProxyAsync),
            ("ExistingEmptyProxyIsSelected", ExistingEmptyProxyIsSelectedAsync),
            ("CancelledConnectHasNoSideEffects", CancelledConnectHasNoSideEffectsAsync),
            ("ConfirmedConnectWaitsForCompletion", ConfirmedConnectWaitsForCompletionAsync),
            ("ConfirmedConnectStartsAfterDialogCloses", ConfirmedConnectStartsAfterDialogClosesAsync),
            ("FailedConnectDoesNotSave", FailedConnectDoesNotSaveAsync),
            ("SuccessfulConnectSavesProfile", SuccessfulConnectSavesProfileAsync),
            ("ConfigurationUsesCurrentForm", ConfigurationUsesCurrentFormAsync),
            ("SuccessfulCallbackShowsStatus", SuccessfulCallbackShowsStatusAsync),
            ("DisconnectedCallbackKeepsError", DisconnectedCallbackKeepsErrorAsync),
            ("SaveFailureIsLogged", SaveFailureIsLoggedAsync),
            ("ClearResetsForm", ClearResetsFormAsync),
            ("CancelledSettingsCommandsDoNothing", CancelledSettingsCommandsDoNothingAsync),
            ("DeleteClearsCredentials", DeleteClearsCredentialsAsync),
            ("EmptyProfileExplainsSetup", EmptyProfileExplainsSetupAsync),
            ("MissingApiHashPreventsConnection", MissingApiHashPreventsConnectionAsync),
            ("InvalidApiIdExplainsSetup", InvalidApiIdExplainsSetupAsync),
            ("ReportedApiErrorExplainsSetup", ReportedApiErrorExplainsSetupAsync),
            ("PhoneCodeErrorExplainsNextStep", PhoneCodeErrorExplainsNextStepAsync),
            ("ReportedPhoneCodeErrorExplainsNextStep", ReportedPhoneCodeErrorExplainsNextStepAsync),
            ("SessionFailureWithoutDialogExplainsRecovery", SessionFailureWithoutDialogExplainsRecoveryAsync),
            ("SessionFailureWithDialogExplainsRecovery", SessionFailureWithDialogExplainsRecoveryAsync),
            ("UnrelatedConnectionErrorIsPreserved", UnrelatedConnectionErrorIsPreservedAsync),
            ("EnglishInstructionsUseApplicationResources", EnglishInstructionsUseApplicationResourcesAsync),
            ("ConnectionExceptionTextCanBeSelected", ConnectionExceptionTextCanBeSelectedAsync),
            ("ConnectionMessageHasSeparateFullWidthRow", ConnectionMessageHasSeparateFullWidthRowAsync),
            ("DeleteRemovesSessionAfterDisconnect", DeleteRemovesSessionAfterDisconnectAsync),
            ("CancelledDeleteKeepsSession", CancelledDeleteKeepsSessionAsync),
            ("SessionResetPreservesProfileAndAllowsNewSession", SessionResetPreservesProfileAndAllowsNewSessionAsync),
            ("CancelledSessionResetKeepsSession", CancelledSessionResetKeepsSessionAsync),
            ("SessionResetDialogSeparatesHeadingAndBody", SessionResetDialogSeparatesHeadingAndBodyAsync),
            ("SessionResetWaitsForDisconnect", SessionResetWaitsForDisconnectAsync),
            ("SessionResetFailureIsShownAndLogged", SessionResetFailureIsShownAndLoggedAsync),
            ("DisconnectFailureStopsDeletion", DisconnectFailureStopsDeletionAsync),
            ("MissingSessionResetSucceeds", MissingSessionResetSucceedsAsync),
            ("SessionResetButtonIsBound", SessionResetButtonIsBoundAsync),
            ("CorruptSessionCanBeResetAfterFailedConstructor", CorruptSessionCanBeResetAfterFailedConstructorAsync),
            ("SessionInspectionUsesFileFormat", SessionInspectionUsesFileFormatAsync),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            App.BusinessLogicManager = new();
            App.VmLocator = new();
            ContentDialog.Confirm = false;
            TgResourceExtensions.Language = "ru-RU";
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}; подключения к Telegram не выполнялись");
        return failures == 0 ? 0 : 1;
    }

    // Проверяем доступность выделения текста исключения в исходной разметке без разрешения редактирования
    private static Task ConnectionExceptionTextCanBeSelectedAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ConnectionPage.xaml")!;
        var page = System.Xml.Linq.XDocument.Load(stream);
        System.Xml.Linq.XNamespace controls = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var field = page.Descendants(controls + "TextBox")
            .Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ConnectionMessage");
        Assert((string?)field.Attribute("IsReadOnly") == "True", "Текст исключения доступен для редактирования");
        Assert(field.AncestorsAndSelf().All(x => (string?)x.Attribute("IsEnabled") != "False"
            && (string?)x.Attribute("IsHitTestVisible") != "False"), "Поле исключения или его контейнер блокирует выделение и копирование текста");
        return Task.CompletedTask;
    }

    // Проверяем сообщение на полной ширине вне колонок центра и общее скрытие конфиденциальных данных
    private static Task ConnectionMessageHasSeparateFullWidthRowAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ConnectionPage.xaml")!;
        var page = System.Xml.Linq.XDocument.Load(stream);
        var field = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ConnectionMessage");
        Assert((string?)field.Attribute("Text") == "{x:Bind ViewModel.DisplayConnectionMessage, Mode=OneWay}", "Сообщение обходит скрытие данных");
        Assert((string?)field.Attribute("TextWrapping") == "Wrap" && field.Parent!.Elements().Count() == 1,
            "Сообщение зажато среди других полей или не переносится");
        Assert(field.Parent!.Parent!.Name.LocalName == "StackPanel", "Сообщение ограничено колонкой центра");
        var vm = CreateViewModel();
        vm.ConnectionMsg = "private error";
        vm.IsDisplaySensitiveData = false;
        Assert(vm.DisplayConnectionMessage == vm.SensitiveData, "Ошибка раскрывает конфиденциальные данные");
        return Task.CompletedTask;
    }

    // Проверяем начальные значения до навигации и отсутствие выполнения пользовательской команды из конструктора
    private static Task ConstructorInitializesModelAsync()
    {
        var vm = CreateViewModel();
        Assert(vm.AppDto is not null, "Модель настроек отсутствует до первой загрузки");
        Assert(vm.DialogRequests == 0, "Конструктор запустил диалог очистки");
        return Task.CompletedTask;
    }

    // Проверяем выбор прокси из только что загруженных настроек
    private static async Task NavigationSelectsSavedProxyAsync()
    {
        var proxy = AddProxy();
        App.BusinessLogicManager.StorageManager.AppRepository.Current.ProxyUid = proxy.Uid;
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        Assert(vm.ProxyVm?.Dto.Uid == proxy.Uid, "Сохранённый прокси не выбран при первом открытии страницы");
    }

    // Проверяем отсутствие потери сохранённого прокси при сохранении без редактирования формы
    private static async Task SavePreservesSelectedProxyAsync()
    {
        var proxy = AddProxy();
        var repository = App.BusinessLogicManager.StorageManager.AppRepository;
        repository.Current.ProxyUid = proxy.Uid;
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.ConfirmDialog = true;
        await vm.AppSaveCommand.ExecuteAsync(null);
        Assert(repository.SaveCalls == 1 && repository.SavedProxyUid == proxy.Uid, "Сохранение формы сбросило прокси");
    }

    // Проверяем выбор существующего пустого элемента при отсутствии подходящего сохранённого прокси
    private static async Task ExistingEmptyProxyIsSelectedAsync()
    {
        App.BusinessLogicManager.StorageManager.ProxyRepository.Items.Add(new() { Uid = Guid.NewGuid(), UserName = "No user" });
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        Assert(vm.ProxyVm is not null && vm.ProxyVm.IsEmptyProxy && vm.ProxiesVms.Contains(vm.ProxyVm), "Пустой прокси есть в списке, но выбор сброшен в null");
    }

    // Проверяем отмену подключения без открытия Избранного и изменения оболочки
    private static async Task CancelledConnectHasNoSideEffectsAsync()
    {
        var vm = CreateViewModel();
        var initial = Guid.NewGuid();
        App.VmLocator.Shell.UidSavedMessages = initial;
        await vm.ClientConnectCommand.ExecuteAsync(true);
        var connection = App.BusinessLogicManager.ConnectClient;
        Assert(connection.ConnectCalls == 0 && connection.SavedMessagesCalls == 0, "Отмена подключения вызвала действие клиента");
        Assert(App.VmLocator.Shell.UidSavedMessages == initial, "Отмена подключения изменила Избранное");
    }

    // Проверяем ожидание длительной подтверждённой операции исходной командой
    private static async Task ConfirmedConnectWaitsForCompletionAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        vm.ConfirmDialog = true;
        SetReadyClient();
        var connection = App.BusinessLogicManager.ConnectClient;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.ConnectGate = gate;
        var operation = vm.ClientConnectCommand.ExecuteAsync(true);
        try
        {
            Assert(connection.ConnectCalls == 1, "Подтверждение не запустило действие");
            Assert(!operation.IsCompleted && vm.ClientConnectCommand.IsRunning, "Команда завершилась раньше подтверждённого подключения");
        }
        finally
        {
            gate.TrySetResult();
            await operation.WaitAsync(TimeSpan.FromSeconds(5));
            if (ContentDialog.LastAction is { } pending) await pending.WaitAsync(TimeSpan.FromSeconds(5));
        }
        Assert(connection.SavedMessagesCalls == 1, "После завершения подключения не создано Избранное");
    }

    // Проверяем выполнение длительной операции после закрытия вопроса
    private static async Task ConfirmedConnectStartsAfterDialogClosesAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        vm.ConfirmDialog = true;
        SetReadyClient();
        await vm.ClientConnectCommand.ExecuteAsync(true);
        var connection = App.BusinessLogicManager.ConnectClient;
        Assert(connection.ConnectCalls == 1 && !connection.StartedWhileDialogOpen, "Подключение началось до закрытия диалога");
    }

    // Проверяем отказ авторизации, который возвращается без исключения
    private static async Task FailedConnectDoesNotSaveAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(App.BusinessLogicManager.ConnectClient.ConnectCalls == 1, "Команда не попыталась выполнить подключение");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0, "Неудачное подключение сохранило настройки");
        Assert(App.BusinessLogicManager.ConnectClient.SavedMessagesCalls == 0, "Неудачное подключение открыло Избранное");
    }

    // Проверяем успешное сохранение профиля и создание Избранного ровно один раз
    private static async Task SuccessfulConnectSavesProfileAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        SetReadyClient();
        await vm.ClientConnectCommand.ExecuteAsync(false);
        var connection = App.BusinessLogicManager.ConnectClient;
        Assert(connection.ConnectCalls == 1 && connection.SavedMessagesCalls == 1, "Успешное подключение не завершило нужные действия ровно один раз");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 1 && vm.AppDto.FirstName == "Test", "Профиль не сохранён");
        Assert(App.VmLocator.Shell.UidSavedMessages == connection.SavedMessagesUid, "Избранное не обновлено в оболочке");
    }

    // Проверяем конфигурационный обратный вызов на текущих данных формы без настоящего клиента
    private static async Task ConfigurationUsesCurrentFormAsync()
    {
        var vm = CreateViewModel();
        var hash = Guid.NewGuid();
        vm.AppDto = new() { ApiId = 42, ApiHash = hash, PhoneNumber = "+10000000000" };
        vm.Password = "test-password";
        vm.VerificationCode = "12345";
        await vm.ClientConnectCommand.ExecuteAsync(false);
        var configuration = App.BusinessLogicManager.ConnectClient.Configuration;
        Assert(configuration["api_id"] == "42" && configuration["api_hash"] == hash.ToString("N"), "Обратный вызов вернул неверные API-параметры");
        Assert(configuration["phone_number"] == vm.AppDto.PhoneNumber && configuration["session_pathname"] == "test.session", "Пути или телефон не переданы");
        Assert(configuration["password"] is null && configuration["verification_code"] is null,
            "Конфигурация подавляет пошаговые запросы Telegram старым кодом или паролем");
        Assert(!vm.DataRequest.Contains("test-password") && !vm.DataRequest.Contains("12345") && !vm.DataRequest.Contains(hash.ToString("N")),
            "Диагностика раскрывает секреты авторизации");
    }

    // Проверяем сообщение об успешном подключении при отсутствии исключений
    private static async Task SuccessfulCallbackShowsStatusAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new();
        SetReadyClient();
        await App.BusinessLogicManager.ConnectClient.NotifyAsync();
        Assert(vm.ConnectionMsg == TgResourceExtensions.GetClientIsConnected() && vm.UserName == "test", "Статус успешного подключения не показан");
    }

    // Проверяем сохранение сообщения RPC при отсутствии клиента
    private static async Task DisconnectedCallbackKeepsErrorAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new();
        vm.Exception.Set(new InvalidOperationException("PHONE_CODE_INVALID"));
        await App.BusinessLogicManager.ConnectClient.NotifyAsync();
        Assert(vm.ConnectionMsg == TgResourceExtensions.GetRpcErrorPhoneCodeInvalid(), "Обновление отключённого клиента очистило сообщение RPC");
        Assert(vm.Exception.IsExist, "Обновление отключённого клиента очистило исходную ошибку");
    }

    // Проверяем запись ошибки сохранения в журнал и отображение её в модели
    private static async Task SaveFailureIsLoggedAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new();
        vm.ConfirmDialog = true;
        App.BusinessLogicManager.StorageManager.AppRepository.FailSave = true;
        await vm.AppSaveCommand.ExecuteAsync(null);
        Assert(vm.Errors.Count == 1 && vm.Exception.IsExist, "Ошибка сохранения не зарегистрирована в журнале и форме");
        Assert(vm.HasConnectionMessage && vm.ConnectionMsg == vm.Exception.Message, "Ошибка сохранения не видна в сообщении центра подключения");
    }

    // Проверяем очистку формы без удаления записи из хранилища
    private static async Task ClearResetsFormAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new() { ApiId = 42 };
        vm.Password = "password";
        vm.VerificationCode = "12345";
        vm.ConfirmDialog = true;
        await vm.AppClearCommand.ExecuteAsync(null);
        Assert(vm.AppDto.ApiId == 0 && vm.Password.Length == 0 && vm.VerificationCode.Length == 0, "Очистка не сбросила форму");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.DeleteCalls == 0, "Очистка формы удалила настройки");
    }

    // Проверяем отмену сохранения, очистки и удаления без изменения модели и репозитория
    private static async Task CancelledSettingsCommandsDoNothingAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new() { ApiId = 42 };
        await vm.AppSaveCommand.ExecuteAsync(null);
        await vm.AppClearCommand.ExecuteAsync(null);
        await vm.AppDeleteCommand.ExecuteAsync(null);
        var repository = App.BusinessLogicManager.StorageManager.AppRepository;
        Assert(vm.AppDto.ApiId == 42 && repository.SaveCalls == 0 && repository.DeleteCalls == 0, "Отменённая команда изменила настройки");
    }

    // Проверяем сброс введённых данных авторизации после удаления настроек
    private static async Task DeleteClearsCredentialsAsync()
    {
        var vm = CreateViewModel();
        vm.Password = "password";
        vm.VerificationCode = "12345";
        vm.DataRequest = "verification_code: 12345";
        vm.ConfirmDialog = true;
        await vm.AppDeleteCommand.ExecuteAsync(null);
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.DeleteCalls == 1, "Удаление не выполнено");
        Assert(vm.Password.Length == 0 && vm.VerificationCode.Length == 0 && vm.DataRequest.Length == 0, "Удаление сохранило данные прежней авторизации в форме");
    }

    // Создаём исходную модель без реального контейнера приложения
    private static TgClientConnectionViewModel CreateViewModel() => new(new TestLoadStateService(), new TestSettingsService(), null!, null!);

    // Добавляем обычный прокси в тестовое хранилище
    private static TgEfProxyEntity AddProxy()
    {
        var proxy = new TgEfProxyEntity { Uid = Guid.NewGuid(), Type = TgEnumProxyType.Socks, UserName = "user" };
        App.BusinessLogicManager.StorageManager.ProxyRepository.Items.Add(proxy);
        return proxy;
    }

    // Подготавливаем готового клиента без вызовов Telegram
    private static void SetReadyClient()
    {
        var connection = App.BusinessLogicManager.ConnectClient;
        connection.Ready = true;
        connection.Me = new TL.User { id = 42, first_name = "Test", username = "test" };
        connection.Client = new TestClient { User = connection.Me };
    }

    // Проверяем наблюдаемый результат исходной модели
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
