namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    private static readonly string[] _resetOptions = [nameof(TgEfSourceDto.IsSaveMessages), nameof(TgEfSourceDto.IsSaveFiles),
        nameof(TgEfSourceDto.IsParsingComments), nameof(TgEfSourceDto.IsJoinFileNameWithMessageId), nameof(TgEfSourceDto.IsDownloadThumbnail),
        nameof(TgEfSourceDto.IsCreatingSubdirectories), nameof(TgEfSourceDto.IsAutoUpdate)];
    private static readonly string[] _otherOptions = [nameof(TgEfSourceDto.IsRewriteMessages), nameof(TgEfSourceDto.IsRewriteFiles),
        nameof(TgEfSourceDto.IsFileNamingByMessage)];

    // Сброс включает семь опций в общем черновике, сохраняя каталог, позицию, потоки и остальные переключатели
    private static async Task ChatResetOptionsAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        SetChatOptions(Chat.Dto, enabled: false);
        Chat.Dto.IsRewriteFiles = true; Chat.Dto.IsFileNamingByMessage = true;
        Chat.Dto.Directory = "downloads/chat"; Chat.Dto.FirstId = 123; Chat.Dto.CountThreads = 7;
        await Download.OnNavigatedToAsync(new() { Parameter = Chat.Dto });
        var command = Chat.GetType().GetProperty("ResetChatSettingsCommand")?.GetValue(Chat) as IRelayCommand;
        Assert(command is not null, "На странице чата отсутствует команда сброса настроек");
        command!.Execute(null);
        Assert(_resetOptions.All(x => ReadChatOption(Chat.Dto, x)), "Сброс не включил все семь опций");
        Assert(ReferenceEquals(Chat.Dto, Download.DownloadDto) && Download.DownloadDto.IsAutoUpdate, "Сброс не обновил общий черновик вкладки");
        Assert(Chat.Dto.Directory == "downloads/chat" && Chat.Dto.FirstId == 123 && Chat.Dto.CountThreads == 7
            && Chat.Dto.IsRewriteFiles && Chat.Dto.IsFileNamingByMessage && !Chat.Dto.IsRewriteMessages, "Сброс затронул другие настройки чата");
        await Chat.SaveChatSettingsCommand.ExecuteAsync(null);
        var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == _source.Uid);
        Assert(_resetOptions.All(x => ReadChatOption(saved, x)) && saved.FirstId == 123 && Chat.Errors.Count == 0,
            "Результат сброса не сохранился в SQLite");
    }

    // Чат без единого выбранного переключателя получает опции сброса при открытии без скрытой записи в БД
    private static async Task EmptyChatOptionsResetOnOpenAsync()
    {
        SetChatOptions(_source, enabled: false);
        await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(_source);
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(_resetOptions.All(x => ReadChatOption(Chat.Dto, x)), "При открытии чата все опции остались выключены");
        Assert(_otherOptions.All(x => !ReadChatOption(Chat.Dto, x)), "Автоматический сброс включил перезапись или имя по сообщению");
        var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == _source.Uid);
        Assert(_resetOptions.All(x => !ReadChatOption(saved, x)), "Открытие чата без команды сохранения изменило БД");
        await Chat.OnNavigatedToAsync(new() { Parameter = Guid.Empty });
        var empty = new TgEfSourceDto();
        Assert(Chat.Dto.Id <= 0 && _resetOptions.All(x => ReadChatOption(Chat.Dto, x) == ReadChatOption(empty, x)),
            "Сброс заполнил настройки отсутствующего чата");
    }

    // Любой выбранный переключатель сохраняет намеренно заданную пользователем комбинацию
    private static async Task SelectedChatOptionsPreservedAsync()
    {
        foreach (var selected in _resetOptions.Concat(_otherOptions))
        {
            SetChatOptions(_source, enabled: false);
            typeof(TgEfSourceDto).GetProperty(selected)!.SetValue(_source, true);
            await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(_source);
            await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(_resetOptions.Concat(_otherOptions).All(x => ReadChatOption(Chat.Dto, x) == (x == selected)),
                $"Открытие чата изменило заданную комбинацию: {selected}");
        }
    }

    // Подтверждённый запуск сохраняет черновик до проверки подключения, даже если сеть уже недоступна
    private static async Task StartSavesBeforeConnectionCheckAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        foreach (var connected in new[] { true, false })
        {
            var directory = connected ? "downloads/connected" : "downloads/disconnected";
            Chat.Dto.Directory = directory; Chat.Dto.IsAutoUpdate = connected; Chat.Dto.IsSaveFiles = !connected;
            var checkedConnection = false; var parsed = false;
            App.BusinessLogicManager.ConnectClient.CheckConnection = async () =>
            {
                checkedConnection = true;
                var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == _source.Uid);
                Assert(saved.Directory == directory && saved.IsAutoUpdate == connected && saved.IsSaveFiles == !connected,
                    "Проверка подключения началась до сохранения настроек чата");
                return connected;
            };
            App.BusinessLogicManager.ConnectClient.Parse = settings =>
            {
                parsed = true;
                Assert(settings.SourceVm.Dto.Directory == directory && ReferenceEquals(settings.SourceVm.Dto, Chat.Dto),
                    "Парсинг получил старые настройки вместо сохранённого черновика");
                return Task.CompletedTask;
            };
            await Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
            Assert(checkedConnection && parsed == connected && Chat.Errors.Count == 0, "Запуск не сохранил настройки или нарушил проверку подключения");
            Assert(Chat.Dto.Directory == directory, "Неудачное подключение стёрло изменения чата");
        }
    }

    // Ошибка сохранения останавливает запуск до сетевой проверки и парсинга
    private static async Task StartDoesNotParseAfterSaveFailureAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Chat.Dto.Uid = Guid.Empty;
        var networkCalled = false;
        App.BusinessLogicManager.ConnectClient.CheckConnection = () => { networkCalled = true; return Task.FromResult(true); };
        App.BusinessLogicManager.ConnectClient.Parse = _ => { networkCalled = true; return Task.CompletedTask; };
        await Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        Assert(!networkCalled && Chat.Errors.Count > 0, "Ошибка сохранения скрыта или после неё началась работа с Telegram");
    }

    // Кнопка загрузки выполняет настоящий переход через службу навигации
    private static Task DownloadNetworkNavigationAsync()
    {
        _navigation.NavigateTo("BeforeNetworkActivity", new object());
        var command = Download.GetType().GetProperty("OpenNetworkActivityCommand")?.GetValue(Download) as IRelayCommand;
        Assert(command is not null, "Нет команды перехода в сетевую активность");
        command!.Execute(null);
        Assert(_navigation.PageKey == "OpenTgResearcherDesktop.ViewModels.TgNetworkActivityViewModel" && _navigation.Parameter is null,
            "Кнопка открывает другую страницу или передаёт лишний параметр");
        return Task.CompletedTask;
    }

    // В общей панели доступен сброс, а вместо списка активных задач находится кнопка сетевой активности
    private static Task ChatActionsMarkupAsync()
    {
        var download = ReadMarkup("TgChatDownloadPage.xaml");
        Assert(!download.Descendants().Attributes().Any(x => x.Value.Contains("StateActiveTasks") || x.Value == "FieldActiveTasks"),
            "На вкладке остался блок активных задач");
        var button = download.Descendants().SingleOrDefault(x => x.Name.LocalName == "Button"
            && ((string?)x.Attribute("Command"))?.Contains("OpenNetworkActivityCommand") == true);
        Assert(button is not null && button.Descendants().Any(x => x.Attributes().Any(a => a.Value == "ActionOpenNetworkActivity")),
            "Нет локализованной кнопки сетевой активности");
        Assert((string?)button!.Attribute("IsEnabled") is null, "Переход к активности блокируется на время парсинга");
        var chat = ReadMarkup("TgChatPage.xaml");
        Assert(chat.Descendants().Any(x => x.Name.LocalName == "Button"
            && ((string?)x.Attribute("Command"))?.Contains("ResetChatSettingsCommand") == true), "Сброс недоступен из общей панели чата");
        return Task.CompletedTask;
    }

    // Выключаем или включаем именно переключатели вкладки загрузки
    private static void SetChatOptions(TgEfSourceDto dto, bool enabled)
    {
        foreach (var name in _resetOptions.Concat(_otherOptions)) typeof(TgEfSourceDto).GetProperty(name)!.SetValue(dto, enabled);
    }

    // Читаем выбранную опцию настоящего DTO
    private static bool ReadChatOption(TgEfSourceDto dto, string name) => (bool)typeof(TgEfSourceDto).GetProperty(name)!.GetValue(dto)!;
}
