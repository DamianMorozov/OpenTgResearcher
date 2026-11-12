namespace OpenTgResearcherDesktop.ConnectionSelfTests;

// Проверяем исходную VM: пошаговый вход, возврат в центр, очистку состояния и русские и английские подписи
internal static partial class Program
{
    private static async Task SignInExistingSessionStartsCenterAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.Profile.Uid = Guid.NewGuid();
        await fixture.ViewModel.OnNavigatedToAsync(null);
        Assert(fixture.ViewModel.SignIn.IsCenter && fixture.Connection.ConnectCalls == 0,
            "Сохранённый профиль навязывает мастер или подключается без команды");
        fixture.ViewModel.OpenSignInCommand.Execute(null);
        Assert(fixture.ViewModel.SignIn.IsApiStep && File.Exists(fixture.Settings.AppSession), "Ручной мастер удаляет сессию");
        SetReadyClient();
        fixture.ViewModel.OpenSignInCommand.Execute(null);
        Assert(fixture.ViewModel.SignIn.IsDoneStep, "Подключённый аккаунт не открыл завершённый мастер");
        await fixture.ViewModel.ClientDisconnectCommand.ExecuteAsync(false);
        await fixture.Connection.NotifyAsync();
        Assert(fixture.ViewModel.SignIn.IsApiStep && !fixture.ViewModel.SignIn.IsDoneStep,
            "Отключённый аккаунт всё ещё показан как успешно подключённый");
    }

    private static async Task SignInWizardIntegrationAsync()
    {
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        Assert(vm.SignIn.IsApiStep, "Новый профиль не открыл мастер");
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(vm.SignIn.IsApiStep && vm.HasConnectionMessage && App.BusinessLogicManager.ConnectClient.ConnectCalls == 0,
            "Пустые реквизиты пропущены или отправлены в Telegram");
        vm.AppDto = CreateValidProfile();
        vm.ApiIdInput = "12345";
        vm.ApiHashInput = "112233445566778899aabbccddeeff00";
        Assert(vm.AppDto.ApiId == 12345 && vm.AppDto.ApiHashString == vm.ApiHashInput,
            "Общие поля мастера не обновляют реквизиты подключения");
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(vm.SignIn.IsPhoneStep && App.BusinessLogicManager.ConnectClient.ConnectCalls == 0, "Локальная проверка отправляет телефон");
        var connection = App.BusinessLogicManager.ConnectClient;
        connection.Client = new();
        connection.LoginStep = input => Task.FromResult<string?>(input switch { null => "verification_code", "12345" => "password", _ => null });
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(vm.SignIn.IsAuthStep && vm.SignIn.IsCodeRequested && !vm.SignIn.CanEditCredentials,
            "Не показано подтверждение или разрешено менять реквизиты ожидающего клиента");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0 && connection.SavedMessagesCalls == 0,
            "Промежуточный шаг сохранён как успешный вход");
        vm.CloseSignInCommand.Execute(null);
        Assert(vm.SignIn.IsCenter && vm.SignIn.HasRequest, "Возврат в центр потерял подтверждение");
        var draft = vm.AppDto;
        await vm.OnNavigatedToAsync(null);
        Assert(ReferenceEquals(vm.AppDto, draft) && vm.AppDto.ApiId == 12345 && vm.SignIn.HasRequest,
            "Возврат на страницу заменил ожидающий профиль данными из БД");
        vm.OpenSignInCommand.Execute(null);
        Assert(vm.SignIn.IsCodeRequested, "Ручной вызов не вернул ожидаемый код");
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(connection.ConnectCalls == 1 && vm.SignIn.IsCodeRequested, "Пустой код отправлен в Telegram");
        vm.VerificationCode = "12345";
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(vm.SignIn.IsPasswordRequested && vm.VerificationCode.Length == 0, "Код не передан или остался в форме");
        vm.Password = "sample-password";
        SetReadyClient();
        await vm.SignInNextCommand.ExecuteAsync(null);
        Assert(vm.SignIn.IsDoneStep && vm.Password.Length == 0 && connection.LoginInputs.SequenceEqual(new string?[] { null, "12345", "sample-password" }),
            "Мастер повторно отправил телефон, потерял пароль или не завершился");
        Assert(App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 1 && connection.SavedMessagesCalls == 1,
            "Успешный вход не сохранил профиль и Избранное ровно один раз");
        vm.OpenSignInCommand.Execute(null);
        Assert(vm.SignIn.IsDoneStep && connection.DisconnectCalls == 0 && connection.ConnectCalls == 3,
            "Ручной мастер отключает аккаунт или заново выполняет вход");
    }

    private static async Task SignInDisconnectResetsRequestAsync()
    {
        var vm = CreateViewModel();
        vm.AppDto = CreateValidProfile();
        var connection = App.BusinessLogicManager.ConnectClient;
        connection.Client = new();
        connection.LoginStep = _ => Task.FromResult<string?>("verification_code");
        await vm.ClientConnectCommand.ExecuteAsync(false);
        Assert(vm.SignIn.HasRequest, "Не создан запрос подтверждения");
        await vm.ClientDisconnectCommand.ExecuteAsync(false);
        await connection.NotifyAsync();
        Assert(!vm.SignIn.HasRequest && vm.SignIn.CanEditCredentials, "Отключение оставило запрос к уже уничтоженному клиенту");
        connection.Client = new();
        await vm.ClientConnectCommand.ExecuteAsync(false);
        vm.ConfirmDialog = true;
        await vm.AppClearCommand.ExecuteAsync(null);
        Assert(!vm.SignIn.HasRequest && connection.Client is null && connection.DisconnectCalls == 2,
            "Очистка реквизитов оставила старый ожидающий клиент");
    }

    private static Task SignInPageResourcesAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ConnectionPage.xaml")!;
        var page = System.Xml.Linq.XDocument.Load(stream);
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            TgResourceExtensions.Language = language;
            foreach (var element in page.Descendants())
            {
                var uid = element.Attributes().FirstOrDefault(x => x.Name.LocalName == "Uid")?.Value;
                if (uid is null) continue;
                var suffix = element.Name.LocalName switch
                {
                    "TextBlock" => ".Text", "TextBox" or "PasswordBox" or "ComboBox" or "Expander" or "ToggleSwitch" => ".Header", _ => ".Content",
                };
                Assert(!string.IsNullOrWhiteSpace((uid + suffix).GetLocalizedResource()), $"Не переведено {language}: {uid}");
            }
        }
        var buttons = page.Descendants().Where(x => x.Name.LocalName == "Button").ToArray();
        Assert(buttons.Any(x => (string?)x.Attribute("Command") == "{x:Bind ViewModel.OpenSignInCommand}"), "Из центра нельзя открыть мастер");
        Assert(buttons.Where(x => x.Attribute("CommandParameter") is not null)
            .All(x => x.Attribute("CommandParameter")!.Value.StartsWith("{StaticResource Connection", StringComparison.Ordinal)),
            "Команда bool получает строковый параметр");
        return Task.CompletedTask;
    }
}
