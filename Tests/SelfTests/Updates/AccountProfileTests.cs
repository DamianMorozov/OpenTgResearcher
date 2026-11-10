namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Изменение файлов используемого аккаунта по-прежнему проходит через освобождение сессии и перезапуск
    private static async Task ActiveProfilePathSaveRestartsAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var original = TgAccountProfiles.ReadActive(settings)!;
        vm.ProfileEditCommand.Execute(null);
        vm.SelectedProfile.StoragePath = Path.Combine(AppContext.BaseDirectory, "active-edited.db");
        vm.SelectedProfile.SessionPath = Path.Combine(AppContext.BaseDirectory, "active-edited.session");
        await vm.ProfileSaveCommand.ExecuteAsync(null);
        var saved = TgAccountProfiles.ReadActive(settings)!;
        Assert(App.Restarts == 1 && saved.Id == original.Id && saved.StoragePath == vm.SelectedProfile.StoragePath
            && saved.SessionPath == vm.SelectedProfile.SessionPath, "Смена файлов активного профиля не запланировала перезапуск");
        Assert(settings.AppStorage == original.StoragePath && settings.AppSession == original.SessionPath,
            "Файлы старого клиента изменены до завершения процесса");
    }

    // Удаление активного профиля не должно превращать обычное сохранение другого профиля в смену аккаунта
    private static async Task DeletedActiveProfileNeedsExplicitApplyAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var original = TgAccountProfiles.ReadActive(settings)!;
        vm.ProfileCreateCommand.Execute(null);
        await vm.ProfileSaveCommand.ExecuteAsync(null);
        vm.SelectedProfile = vm.Profiles.Single(x => x.Id == original.Id);
        await vm.ProfileDeleteCommand.ExecuteAsync(null);
        await vm.SettingsSaveCommand.ExecuteAsync(null);
        Assert(App.Restarts == 0 && TgAccountProfiles.ReadActive(settings) == original
            && vm.ProfileMessage.Contains("Применить профиль"), "Удаление активного профиля неявно переключило аккаунт при сохранении");
    }

    // Сохранение другого профиля записывает все поля, не переключая используемый аккаунт
    private static async Task InactiveProfileSaveKeepsAccountAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var original = TgAccountProfiles.ReadActive(settings)!;
        vm.ProfileCreateCommand.Execute(null);
        vm.SelectedProfile.Name = "Сохранённый второй";
        vm.SelectedProfile.StoragePath = Path.Combine(AppContext.BaseDirectory, "inactive.db");
        vm.SelectedProfile.SessionPath = Path.Combine(AppContext.BaseDirectory, "inactive.session");
        await vm.ProfileSaveCommand.ExecuteAsync(null);
        Assert(App.Restarts == 0 && TgAccountProfiles.ReadActive(settings) == original,
            "Сохранение неактивного профиля переключило аккаунт или запустило перезапуск");
        Assert(!vm.IsProfileEditing && settings.AppStorage == original.StoragePath && settings.AppSession == original.SessionPath,
            "Сохранение не завершило редактирование или изменило открытые файлы");
        var restored = new TgAccountProfiles(settings, "Профиль по умолчанию");
        var saved = restored.Profiles.Single(x => x.Id == vm.SelectedProfile.Id);
        Assert(saved.Name == vm.SelectedProfile.Name && saved.StoragePath == vm.SelectedProfile.StoragePath
            && saved.SessionPath == vm.SelectedProfile.SessionPath, "Изменения другого профиля не сохранены");
        vm.ProfileEditCommand.Execute(null);
        vm.SelectedProfile.Name = "Второй изменённый";
        vm.SelectedProfile.StoragePath = Path.Combine(AppContext.BaseDirectory, "inactive-edited.db");
        vm.SelectedProfile.SessionPath = Path.Combine(AppContext.BaseDirectory, "inactive-edited.session");
        await vm.SettingsSaveCommand.ExecuteAsync(null);
        var edited = settings.ReadSetting<TgAccountProfiles.State>(TgAccountProfiles.SettingsKey)!.Profiles.Single(x => x.Id == saved.Id);
        Assert(App.Restarts == 0 && TgAccountProfiles.ReadActive(settings) == original && !vm.IsProfileEditing
            && edited.Name == vm.SelectedProfile.Name && edited.StoragePath == vm.SelectedProfile.StoragePath
            && edited.SessionPath == vm.SelectedProfile.SessionPath,
            "Общее сохранение не записало изменения неактивного профиля или переключило аккаунт");
    }

    // Режимы управления сохраняют явные границы между просмотром, черновиком, отменой и сохранением
    private static async Task ProfileEditModeLifecycleAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var original = TgAccountProfiles.ReadActive(settings)!;
        Assert(!vm.IsProfileEditing && vm.CanSelectProfile && vm.ProfileEditCommand.CanExecute(null)
            && !vm.ProfileCancelEditCommand.CanExecute(null), "Первое открытие не соответствует режиму просмотра");
        vm.ProfileEditCommand.Execute(null);
        Assert(vm.IsProfileEditing && !vm.CanSelectProfile && !vm.ProfileCreateCommand.CanExecute(null)
            && !vm.ProfileEditCommand.CanExecute(null) && vm.ProfileCancelEditCommand.CanExecute(null),
            "Начало редактирования не изменило доступность управления");
        vm.SelectedProfile.Name = "Несохранённый";
        vm.SelectedProfile.StoragePath = Path.Combine(AppContext.BaseDirectory, "draft.db");
        vm.SelectedProfile.SessionPath = Path.Combine(AppContext.BaseDirectory, "draft.session");
        ContentDialog.Confirm = false;
        await vm.ProfileSaveCommand.ExecuteAsync(null);
        Assert(vm.IsProfileEditing && vm.SelectedProfile.Name == "Несохранённый" && TgAccountProfiles.ReadActive(settings) == original,
            "Отмена подтверждения потеряла черновик или сохранила изменения");
        vm.ProfileCancelEditCommand.Execute(null);
        Assert(!vm.IsProfileEditing && vm.CanSelectProfile && vm.SelectedProfile.Name == original.Name
            && vm.SelectedProfile.StoragePath == original.StoragePath && vm.SelectedProfile.SessionPath == original.SessionPath,
            "Отмена не вернула все поля профиля в режим просмотра");
        vm.ProfileEditCommand.Execute(null);
        vm.SelectedProfile.Name = "Новое название";
        ContentDialog.Confirm = true;
        await vm.ProfileSaveCommand.ExecuteAsync(null);
        Assert(!vm.IsProfileEditing && vm.CanSelectProfile && TgAccountProfiles.ReadActive(settings)!.Name == "Новое название"
            && App.Restarts == 0, "Сохранение названия не завершило редактирование или вызвало лишний перезапуск");
        vm.ProfileCreateCommand.Execute(null);
        var draft = vm.SelectedProfile;
        Assert(vm.IsProfileEditing && vm.Profiles.Count == 2, "Новый профиль не перешёл в режим редактирования");
        vm.ProfileCancelEditCommand.Execute(null);
        Assert(vm.Profiles.Count == 1 && vm.SelectedProfile.Id == original.Id && !vm.IsProfileEditing
            && !File.Exists(draft.StoragePath) && !File.Exists(draft.SessionPath), "Отмена создания оставила лишнюю запись или создала файлы");
    }

    // Карточка явно показывает режим, действие отмены и ограничение скрытых конфиденциальных полей
    private static Task ProfileEditModeLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(stream);
        XNamespace l = "using:WinUI3Localizer";
        foreach (var uid in new[] { "ProfileViewMode", "ProfileEditMode", "ProfileViewHint", "ProfileEditHint", "ProfileSensitiveEditHint" })
            Assert(page.Descendants().Any(x => x.Attribute(l + "Uids.Uid")?.Value == uid), $"Нет пояснения режима {uid}");
        var cancel = page.Descendants().SingleOrDefault(x => x.Attribute("Command")?.Value.Contains("ProfileCancelEditCommand") == true);
        Assert(cancel is not null && cancel.Ancestors().Any(x => x.Attribute("Visibility")?.Value.Contains("IsProfileEditing") == true),
            "Отмена редактирования не связана с режимом карточки");
        var selector = page.Descendants().Single(x => x.Attribute("AutomationProperties.AutomationId")?.Value == "ProfileSelector");
        Assert(selector.Attribute("IsEnabled")?.Value.Contains("CanSelectProfile") == true,
            "Выбор другого профиля остаётся доступен при редактировании");
        var save = page.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ProfileSave");
        var apply = page.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ProfileApply");
        Assert(save.Attribute("Command")?.Value.Contains("ProfileSaveCommand") == true
            && apply.Attribute("Command")?.Value.Contains("ProfileApplyCommand") == true,
            "Сохранение карточки и смена аккаунта используют одну команду");
        foreach (var input in page.Descendants().Where(x => x.Name.LocalName == "TextBox"
            && x.Attribute("Text")?.Value.Contains("SelectedProfile.") == true))
            Assert(input.Attribute("IsReadOnly")?.Value.Contains("IsProfileEditing") == true, "Поле редактируется в режиме просмотра");
        return Task.CompletedTask;
    }

    // Изменение черновика не переключает аккаунт, отмена и ошибка завершения сохраняют прежние пути
    private static async Task SettingsProfileSwitchAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var original = TgAccountProfiles.ReadActive(settings)!;
        vm.ProfileCreateCommand.Execute(null);
        Assert(vm.Profiles.Count == 2 && vm.IsProfileEditing && !vm.ProfileDeleteCommand.CanExecute(null), "Не создан редактируемый профиль");
        Assert(settings.AppStorage == original.StoragePath && settings.AppSession == original.SessionPath,
            "Создание черновика изменило используемый аккаунт");
        ContentDialog.Confirm = false;
        await vm.ProfileApplyCommand.ExecuteAsync(null);
        Assert(App.Restarts == 0 && TgAccountProfiles.ReadActive(settings)!.Id == original.Id, "Отказ применил профиль");
        ContentDialog.Confirm = true;
        App.ProfileRestartError = new IOException("Ошибка остановки операций");
        await vm.ProfileApplyCommand.ExecuteAsync(null);
        Assert(App.Restarts == 0 && TgAccountProfiles.ReadActive(settings)!.Id == original.Id && vm.ProfileMessage.Contains("Ошибка остановки"),
            "Ошибка остановки переключила профиль или осталась без сообщения");
        App.ProfileRestartError = null;
        await vm.ProfileApplyCommand.ExecuteAsync(null);
        var applied = TgAccountProfiles.ReadActive(settings)!;
        Assert(App.Restarts == 1 && applied.Id == vm.SelectedProfile.Id && applied.StoragePath == vm.SelectedProfile.StoragePath
            && applied.SessionPath == vm.SelectedProfile.SessionPath, "Перезапуск не получил выбранную пару файлов");
        Assert(settings.AppStorage == original.StoragePath && settings.AppSession == original.SessionPath,
            "Пути старого клиента изменились до завершения процесса");
        var directory = Path.GetDirectoryName(applied.StoragePath)!;
        if (Directory.Exists(directory)) Directory.Delete(directory);
    }

    // Заставка переносит исходные пути, продолжает с текущим профилем и подтверждает перезапуск для другого
    private static async Task SplashProfilesMigrationAndSelectionAsync()
    {
        var settings = new ITgSettingsService();
        using var splash = new TgSplashScreenViewModel(settings);
        var original = splash.SelectedProfile;
        Assert(splash.Profiles.Count == 1 && original.Name == "Профиль по умолчанию"
            && original.StoragePath == settings.AppStorage && original.SessionPath == settings.AppSession,
            "Заставка не перенесла текущую пару файлов в профиль по умолчанию");
        Assert(!splash.ContinueCommand.CanExecute(null), "Продолжение доступно до окончания загрузки");
        var continues = 0;
        splash.BackToMainWindow = () => continues++;
        splash.IsLoadComplete = true;
        await splash.ContinueCommand.ExecuteAsync(null);
        Assert(continues == 1 && splash.SettingsLoads == 1 && App.Restarts == 0, "Текущий профиль не продолжил запуск без перезапуска");
        var catalog = new TgAccountProfiles(settings, "Профиль по умолчанию");
        var second = catalog.Create("Второй", Path.Combine(AppContext.BaseDirectory, "second.db"), Path.Combine(AppContext.BaseDirectory, "second.session"));
        catalog.Save(original.Id);
        using var nextSplash = new TgSplashScreenViewModel(settings) { IsLoadComplete = true, BackToMainWindow = () => continues++ };
        nextSplash.SelectedProfile = nextSplash.Profiles.Single(x => x.Id == second.Id);
        ContentDialog.Confirm = false;
        await nextSplash.ContinueCommand.ExecuteAsync(null);
        Assert(continues == 1 && App.Restarts == 0 && TgAccountProfiles.ReadActive(settings)!.Id == original.Id,
            "Отмена выбора профиля заставки открыла другой аккаунт");
        ContentDialog.Confirm = true;
        await nextSplash.ContinueCommand.ExecuteAsync(null);
        Assert(continues == 1 && App.Restarts == 1 && TgAccountProfiles.ReadActive(settings)!.Id == second.Id,
            "Выбор другого профиля не запланировал запуск с новой парой файлов");
    }

    // На заставке выбранный профиль связан со списком, исходный профиль имеет запрошенное пользователем имя
    private static Task SplashProfileLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSplashScreenPage.xaml")!;
        var page = XDocument.Load(stream);
        Assert(page.Descendants().Any(x => x.Name.LocalName == "ComboBox"
            && x.Attribute("ItemsSource")?.Value.Contains("Profiles") == true
            && x.Attribute("SelectedItem")?.Value.Contains("SelectedProfile") == true), "На заставке отсутствует выбор профиля");
        Localizer.Language = "ru-RU";
        Assert("ProfileDefaultName".GetLocalizedResource() == "Профиль по умолчанию", "Неверное имя автоматически созданного профиля");
        return Task.CompletedTask;
    }

    // Переносим прежние пути, сохраняем изменения и запрещаем удаление последнего профиля без удаления файлов
    private static Task AccountProfilesLifecycleAsync()
    {
        var settings = new ITgSettingsService();
        var catalog = new TgAccountProfiles(settings, "Основной");
        var original = catalog.Profiles[0];
        Assert(catalog.Profiles.Count == 1 && original.Name == "Основной", "Текущие настройки не перенесены в первый профиль");
        var lastRejected = false;
        try { catalog.Delete(original.Id); }
        catch (InvalidOperationException) { lastRejected = true; }
        Assert(lastRejected && catalog.Profiles.Count == 1, "Удалён единственный профиль");
        var directory = Path.Combine(AppContext.BaseDirectory, "profile-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var storage = Path.Combine(directory, "example.db");
        var session = Path.Combine(directory, "example.session");
        File.WriteAllText(storage, "database marker");
        File.WriteAllText(session, "session marker");
        try
        {
            var created = catalog.Create("Рабочий", storage, session);
            Assert(catalog.Profiles.Count == 2 && created.Id != original.Id, "Не создан независимый профиль");
            created.Name = "Личный";
            catalog.Save(created.Id);
            var restored = new TgAccountProfiles(settings, "Основной");
            Assert(restored.Profiles.Count == 2 && restored.ActiveProfileId == created.Id, "Не сохранены список и выбранный профиль");
            var active = restored.Profiles[1];
            Assert(active.Name == "Личный" && active.StoragePath == storage && active.SessionPath == session, "Потеряны поля профиля");
            restored.Delete(active.Id);
            restored.Save(original.Id);
            Assert(restored.Profiles.Count == 1 && File.ReadAllText(storage) == "database marker"
                && File.ReadAllText(session) == "session marker", "Удаление профиля затронуло базу или файл сессии");
        }
        finally { File.Delete(storage); File.Delete(session); Directory.Delete(directory); }
        return Task.CompletedTask;
    }

    // Общая страница содержит выбор профиля, его имя, команды управления и пояснение применения
    private static Task AccountProfilesLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(stream);
        foreach (var command in new[] { "ProfileCreateCommand", "ProfileEditCommand", "ProfileDeleteCommand", "ProfileApplyCommand" })
            Assert(page.Descendants().Any(x => x.Attribute("Command")?.Value.Contains(command) == true), $"Отсутствует команда {command}");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "ComboBox" && x.Attribute("ItemsSource")?.Value.Contains("Profiles") == true),
            "Нет выбора профиля");
        foreach (var field in new[] { "Name", "StoragePath", "SessionPath" })
            Assert(page.Descendants().Any(x => x.Name.LocalName == "TextBox"
                && x.Attribute("Text")?.Value.Contains($"SelectedProfile.{field}") == true), $"Не редактируется поле профиля {field}");
        Localizer.Language = "ru-RU";
        Assert("SettingRecording".GetLocalizedResource() == "Профиль", "Блок профиля использует старое название");
        using var splashStream = typeof(Program).Assembly.GetManifestResourceStream("TgSplashScreenPage.xaml")!;
        var splash = XDocument.Load(splashStream);
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            var resources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Strings", language, "Resources.resw"));
            foreach (var control in page.Descendants().Concat(splash.Descendants()))
            {
                var uid = control.Attributes().FirstOrDefault(x => x.Name.LocalName.EndsWith("Uid"))?.Value;
                if (uid?.StartsWith("Profile", StringComparison.Ordinal) != true) continue;
                var property = control.Name.LocalName switch { "TextBlock" => "Text", "TextBox" => "Header", _ => "Content" };
                Assert(resources.Root!.Elements("data").Any(x => x.Attribute("name")?.Value == $"{uid}.{property}"
                    && !string.IsNullOrWhiteSpace(x.Element("value")?.Value)), $"Нет перевода {language}: {uid}.{property}");
            }
        }
        return Task.CompletedTask;
    }
}
