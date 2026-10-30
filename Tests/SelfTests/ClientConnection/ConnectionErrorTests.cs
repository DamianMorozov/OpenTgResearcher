namespace OpenTgResearcherDesktop.ConnectionSelfTests;

internal static partial class Program
{
    // Проверяем подсказку при исключении из команды подключения
    private static Task PhoneCodeErrorExplainsNextStepAsync() => CheckPhoneCodeInstructionsAsync(isCallback: false);

    // Проверяем подсказку при сообщении об ошибке через обратный вызов авторизации
    private static Task ReportedPhoneCodeErrorExplainsNextStepAsync() => CheckPhoneCodeInstructionsAsync(isCallback: true);

    // Проверяем инструкцию из ресурсов в обоих полях страницы и отсутствие продолжения неудачного входа
    private static async Task CheckPhoneCodeInstructionsAsync(bool isCallback)
    {
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            App.BusinessLogicManager = new();
            TgResourceExtensions.Language = language;
            var vm = CreateViewModel();
            vm.AppDto = CreateValidProfile();
            vm.ConfirmDialog = true;
            var error = new InvalidOperationException("400 PHONE_CODE_INVALID");
            var connection = App.BusinessLogicManager.ConnectClient;
            if (isCallback) connection.ReportedException = error;
            else connection.ConnectException = error;
            await vm.ClientConnectCommand.ExecuteAsync(true);
            var message = vm.Exception.Message;
            var expectedParts = language == "ru-RU"
                ? new[] { "Скопируйте", "авторизованного приложения Telegram", "мобильном", "Код логина", "Подключить Telegram" }
                : new[] { "Copy", "Telegram", "logged in", "mobile", "Login code", "Connect" };
            Assert(expectedParts.All(part => message.Contains(part, StringComparison.Ordinal)),
                $"Нет инструкции получения и ввода кода Telegram: {language}");
            Assert(!message.Contains("PHONE_CODE_INVALID") && vm.ConnectionMsg == message && vm.Exception.IsExist,
                "Техническая ошибка показана вместо инструкции или поля статуса и исключения различаются");
            Assert(connection.ConnectCalls == 1 && connection.SavedMessagesCalls == 0
                && App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0, "Неудачный вход повторён или сохранён как успешный");
            if (!isCallback) Assert(vm.Errors.Contains(error), "Исходное исключение потеряно в журнале");
        }
    }

    // Проверяем подсказку для новой БД до создания сетевого клиента
    private static async Task EmptyProfileExplainsSetupAsync()
    {
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(App.BusinessLogicManager.ConnectClient.ConnectCalls == 0, "Пустой профиль передан сетевому клиенту");
        AssertSetupInstructions(vm.Exception.Message);
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0, "Пустой профиль сохранён");
    }

    // Проверяем отсутствие сетевого запроса при пустом хеше
    private static async Task MissingApiHashPreventsConnectionAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = new() { ApiId = 42 };
        vm.ConfirmDialog = true;
        await vm.ClientConnectCommand.ExecuteAsync(true);
        Assert(App.BusinessLogicManager.ConnectClient.ConnectCalls == 0, "Пустой API hash передан сетевому клиенту");
        AssertSetupInstructions(vm.Exception.Message);
    }

    // Проверяем объяснение ошибки API, полученной при создании клиента
    private static async Task InvalidApiIdExplainsSetupAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        var error = new InvalidOperationException("Connection API ID Invalid");
        App.BusinessLogicManager.ConnectClient.ConnectException = error;
        await vm.ClientConnectCommand.ExecuteAsync(false);
        AssertSetupInstructions(vm.Exception.Message);
        Assert(vm.Errors.Contains(error), "Исходная ошибка API потеряна в журнале");
    }

    // Проверяем обратный вызов авторизации, при котором исключение не выходит из команды
    private static async Task ReportedApiErrorExplainsSetupAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        App.BusinessLogicManager.ConnectClient.ReportedException = new InvalidOperationException("400 API_ID_INVALID");
        await vm.ClientConnectCommand.ExecuteAsync(false);
        AssertSetupInstructions(vm.Exception.Message);
        Assert(vm.ConnectionMsg == vm.Exception.Message, "Статус подключения не содержит объяснение ошибки API");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0, "Неудачная авторизация сохранила профиль");
    }

    // Проверяем обработку ошибки чтения сессии при подключении без вопроса
    private static Task SessionFailureWithoutDialogExplainsRecoveryAsync() => CheckSessionFailureAsync(isQuestion: false);

    // Проверяем тот же путь при подтверждении подключения
    private static Task SessionFailureWithDialogExplainsRecoveryAsync() => CheckSessionFailureAsync(isQuestion: true);

    // Получаем реальное исключение WTelegram на неверном файле без обращения к сети
    private static async Task CheckSessionFailureAsync(bool isQuestion)
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        vm.ConfirmDialog = true;
        var bytes = new byte[248];
        using var session = new MemoryStream(bytes, writable: false);
        Exception? error = null;
        try
        {
            using var client = new WTelegram.Client(key => key switch
            {
                "api_id" => "42",
                "api_hash" => "00112233445566778899aabbccddeeff",
                _ => null,
            }, session);
        }
        catch (Exception ex) { error = ex; }
        Assert(error?.Message.Contains("Exception while reading session file", StringComparison.Ordinal) == true,
            "WTelegram не воспроизвёл ошибку чтения сессии");
        var connection = App.BusinessLogicManager.ConnectClient;
        connection.ConnectException = error;
        await vm.ClientConnectCommand.ExecuteAsync(isQuestion);
        var message = vm.Exception.Message;
        Assert(message.Contains("Настройки") && message.Contains("Сессия") && message.Contains("test.session"),
            "Не показаны место настройки и путь проблемной сессии");
        Assert(message.Contains("API ID") && message.Contains("API hash") && message.Contains("новый") && message.Contains("код"),
            "Не объяснены причина и создание новой сессии");
        Assert(!message.Contains("Padding is invalid"), "Техническая ошибка осталась вместо инструкции");
        Assert(connection.ConnectCalls == 1, "Неизменённая сессия прочитана повторно");
        Assert(vm.Errors.Contains(error!), "Исходная ошибка сессии потеряна в журнале");
        Assert(bytes.All(value => value == 0), "Исходная сессия изменена");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0 && connection.SavedMessagesCalls == 0,
            "Ошибка сессии не остановила сохранение и открытие Избранного");
    }

    // Проверяем сохранение неизвестной ошибки без ошибочной диагностики сессии
    private static async Task UnrelatedConnectionErrorIsPreservedAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        var error = new IOException("Тестовая ошибка транспорта");
        App.BusinessLogicManager.ConnectClient.ConnectException = error;
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(vm.Exception.Message == error.Message && vm.Errors.Contains(error), "Неизвестная ошибка подменена или потеряна");
    }

    // Проверяем английские ресурсы на том же исходном пути обработки ошибок
    private static async Task EnglishInstructionsUseApplicationResourcesAsync()
    {
        TgResourceExtensions.Language = "en-US";
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        var connection = App.BusinessLogicManager.ConnectClient;
        connection.ConnectException = new InvalidOperationException("API_HASH_INVALID");
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(vm.Exception.Message.Contains("API development tools") && vm.Exception.Message.Contains("Settings → Files → Storage"),
            "Английская подсказка API не загружена из ресурсов приложения");
        connection.ConnectException = new InvalidOperationException("Exception while reading session file: Padding is invalid and cannot be removed.");
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(vm.Exception.Message.Contains("Settings → Files → Session") && vm.Exception.Message.Contains("test.session")
            && vm.Exception.Message.Contains("Verification code"), "Английская подсказка сессии не объясняет восстановление");
        Assert(connection.ConnectCalls == 2, "Подключение бездействует или повторяется без команды пользователя");
    }

    // Проверяем конкретные действия пользователя, а не только наличие сообщения
    private static void AssertSetupInstructions(string message)
    {
        Assert(message.Contains("Подключение") && message.Contains("АПИ ИД") && message.Contains("АПИ хеш")
            && message.Contains("my.telegram.org") && message.Contains("Номер телефона")
            && message.Contains("Настройки") && message.Contains("Хранилище") && message.Contains("перезапустите"),
            "Нет понятной инструкции по заполнению API-параметров и выбору прежней БД");
    }

    // Создаём синтетический профиль без пользовательских реквизитов
    private static TgEfAppDto CreateValidProfile() => new() { ApiId = 42, ApiHash = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff") };
}
