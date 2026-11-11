namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Паспорт хранилища, просмотр данных и обслуживание выбранного профиля </summary>
public sealed partial class TgStorageViewModel : TgPageViewModelBase
{
    private readonly TgStorageInspector _inspector;
    private TgStorageInspector.Snapshot? _snapshot;
    private int _offset;
    private long _total;
    private bool _active;
    private const int PageSize = 50;
    private const string BackupPreference = "StorageBackupBeforeMaintenance";

    public ObservableCollection<StorageTable> Tables { get; } = [];
    public ObservableCollection<StorageRecord> Records { get; } = [];
    public ObservableCollection<StorageBackup> Backups { get; } = [];
    [ObservableProperty] public partial StorageTable? SelectedTable { get; set; }
    [ObservableProperty] public partial StorageRecord? SelectedRecord { get; set; }
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(DeleteBackupCommand))]
    public partial StorageBackup? SelectedBackup { get; set; }
    [ObservableProperty] public partial string Search { get; set; } = string.Empty;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasMessage))]
    public partial string Message { get; private set; } = string.Empty;
    [ObservableProperty] public partial bool IsBusy { get; private set; }
    [ObservableProperty] public partial bool BackupBeforeMaintenance { get; set; }
    public IAsyncRelayCommand CreateBackupCommand { get; }
    public IAsyncRelayCommand CheckCommand { get; }
    public IAsyncRelayCommand CompactCommand { get; }
    public IAsyncRelayCommand DisableAutoUpdateCommand { get; }
    public IAsyncRelayCommand RestoreCommand { get; }
    public IAsyncRelayCommand DeleteBackupCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand PreviousCommand { get; }
    public IAsyncRelayCommand NextCommand { get; }
    public string ProfileName => TgAccountProfiles.ReadActive(SettingsService)?.Name ?? "ProfileDefaultName".GetLocalizedResource();
    public string FileName => IsDisplaySensitiveData ? Path.GetFileName(SettingsService.AppStorage) : SensitiveData;
    public string StoragePath => IsDisplaySensitiveData ? SettingsService.AppStorage : SensitiveData;
    public string DatabaseSize => IsDisplaySensitiveData ? Size(_snapshot?.Bytes ?? 0) : SensitiveData;
    public string ChatCount => Metric("SOURCES");
    public string MessageCount => Metric("MESSAGES");
    public string LastBackup => _snapshot?.Backups.FirstOrDefault()?.Created.ToString("g") ?? "StorageNoBackup".GetLocalizedResource();
    public string PageCaption => string.Format(CultureInfo.CurrentCulture, "StoragePageRange".GetLocalizedResource(),
        _total == 0 ? 0 : _offset + 1, Math.Min(_offset + PageSize, _total), _total);
    public bool IsIdle => !IsBusy;
    public bool HasMessage => !string.IsNullOrEmpty(Message);
    public bool HasRecords => Records.Count > 0;
    public bool HasBackups => Backups.Count > 0;

    /// <summary> Создаёт независимый проводник без обращения к Telegram </summary>
    public TgStorageViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgStorageViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStorageViewModel))
    {
        _inspector = new(App.GetService<Microsoft.Extensions.Logging.ILogger<TgStorageInspector>>());
        BackupBeforeMaintenance = settingsService.ReadSetting<bool?>(BackupPreference) ?? true;
        CreateBackupCommand = new AsyncRelayCommand(() => RunAsync(CreateBackupAsync));
        CheckCommand = new AsyncRelayCommand(() => RunAsync(CheckAsync));
        CompactCommand = new AsyncRelayCommand(() => MaintainAsync("StorageCompactTitle", () => _inspector.CompactAsync(SettingsService.AppStorage)));
        DisableAutoUpdateCommand = new AsyncRelayCommand(() => MaintainAsync("StorageDisableAutoTitle", () => _inspector.DisableAutoUpdateAsync(SettingsService.AppStorage)));
        RestoreCommand = new AsyncRelayCommand(RestoreAsync);
        DeleteBackupCommand = new AsyncRelayCommand(DeleteBackupAsync, () => !IsBusy && SelectedBackup is not null && IsDisplaySensitiveData);
        SearchCommand = new AsyncRelayCommand(() => LoadRecordsAsync(reset: true));
        PreviousCommand = new AsyncRelayCommand(() => MovePageAsync(-PageSize), () => !IsBusy && _offset > 0);
        NextCommand = new AsyncRelayCommand(() => MovePageAsync(PageSize), () => !IsBusy && _offset + PageSize < _total);
        PropertyChanged += OnStoragePropertyChanged;
    }

    /// <summary> Читает паспорт при открытии страницы </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => ReloadUiAsync();
    /// <summary> Обновляет паспорт и текущую выборку без сетевого подключения </summary>
    public override Task ReloadUiAsync() => RunAsync(async () =>
    {
        await RefreshSnapshotAsync();
        if (SelectedTable is not null) await ReadRecordsCoreAsync();
    });

    /// <summary> Подписывается на смену языка только на время отображения страницы </summary>
    public void Activate()
    {
        if (_active) return;
        _active = true;
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        RefreshLabels();
    }

    /// <summary> Освобождает подписку при уходе со страницы </summary>
    public void Deactivate()
    {
        if (!_active) return;
        _active = false;
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
    }

    /// <summary> Освобождает подписки модели </summary>
    public override void ReleaseManagedResources()
    {
        Deactivate();
        PropertyChanged -= OnStoragePropertyChanged;
        base.ReleaseManagedResources();
    }

    // Обновляем динамические подписи без повторного чтения базы
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(RefreshLabels);

    // Применяем текущую культуру к карточкам и счётчикам
    private void RefreshLabels()
    {
        foreach (var property in new[] { nameof(ProfileName), nameof(LastBackup), nameof(PageCaption), nameof(ChatCount), nameof(MessageCount) })
            OnPropertyChanged(property);
        foreach (var table in Tables) table.RefreshLabels();
    }

    partial void OnBackupBeforeMaintenanceChanged(bool value) => SettingsService.SaveSetting(BackupPreference, value);
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsIdle));
        PreviousCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        DeleteBackupCommand.NotifyCanExecuteChanged();
    }

    // При переключении приватности обновляем путь немедленно
    private void OnStoragePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsDisplaySensitiveData)) return;
        DeleteBackupCommand.NotifyCanExecuteChanged();
        foreach (var property in new[] { nameof(StoragePath), nameof(FileName), nameof(DatabaseSize), nameof(ChatCount), nameof(MessageCount) })
            OnPropertyChanged(property);
        foreach (var table in Tables) table.SetVisibility(IsDisplaySensitiveData);
    }

    // Обновляем агрегаты и каталог, сохраняя выбранную таблицу
    private async Task RefreshSnapshotAsync()
    {
        _snapshot = await _inspector.InspectAsync(SettingsService.AppStorage);
        var selected = SelectedTable?.Name;
        Tables.Clear();
        var order = new[] { "SOURCES", "MESSAGES", "USERS", "DOCUMENTS", "CHAT_USERS", "STORIES", "FILTERS", "PROXIES", "APPS", "LICENSES",
            "MESSAGES_RELATIONS", "VERSIONS" };
        foreach (var table in _snapshot.Tables.OrderBy(x => Array.IndexOf(order, x.Name)))
            Tables.Add(new(table.Name, table.Count, IsDisplaySensitiveData));
        SelectedTable = Tables.FirstOrDefault(x => x.Name == selected) ?? Tables.FirstOrDefault(x => x.Name == "SOURCES") ?? Tables.FirstOrDefault();
        Backups.Clear();
        foreach (var backup in _snapshot.Backups) Backups.Add(new(backup.Path, backup.Bytes, backup.Created));
        SelectedBackup = Backups.FirstOrDefault();
        foreach (var property in new[] { nameof(ProfileName), nameof(FileName), nameof(StoragePath), nameof(DatabaseSize), nameof(ChatCount),
            nameof(MessageCount), nameof(LastBackup), nameof(HasBackups) }) OnPropertyChanged(property);
    }

    /// <summary> Загружает выбранную таблицу после выбора пользователем </summary>
    public Task LoadRecordsAsync(bool reset = true) => RunAsync(async () =>
    {
        if (reset) _offset = 0;
        await ReadRecordsCoreAsync();
    });

    // Читаем одну порцию и создаём карточку полей выбранной строки
    private async Task ReadRecordsCoreAsync()
    {
        Records.Clear();
        SelectedRecord = null;
        if (SelectedTable is null) return;
        var page = await _inspector.ReadAsync(SettingsService.AppStorage, SelectedTable.Name, Search.Trim(), _offset, PageSize);
        _total = page.Total;
        if (_offset >= _total && _offset > 0)
        {
            _offset = Math.Max(0, ((int)Math.Min(_total, int.MaxValue) - 1) / PageSize * PageSize);
            page = await _inspector.ReadAsync(SettingsService.AppStorage, SelectedTable.Name, Search.Trim(), _offset, PageSize);
        }
        foreach (var row in page.Rows)
        {
            var fields = page.Columns.Select((name, i) => new StorageField(name, row[i])).ToArray();
            var title = fields.FirstOrDefault(x => x.Name is "TITLE" or "NAME" or "MESSAGE" or "FILE_NAME" or "FIRST_NAME")?.Value ?? row.FirstOrDefault() ?? "—";
            var id = fields.FirstOrDefault(x => x.Name == "ID")?.Value ?? row.FirstOrDefault() ?? "—";
            Records.Add(new(title.Length > 160 ? title[..160] + "…" : title, id, fields));
        }
        SelectedRecord = Records.FirstOrDefault();
        OnPropertyChanged(nameof(PageCaption));
        OnPropertyChanged(nameof(HasRecords));
    }

    // Меняем страницу только после окончания предыдущего запроса
    private Task MovePageAsync(int delta) => RunAsync(async () => { _offset = Math.Max(0, _offset + delta); await ReadRecordsCoreAsync(); });

    // Показываем ошибку рядом с действием и не выпускаем исключение в обработчик интерфейса
    private async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { Message = string.Empty; await action(); }
        catch (Exception ex) { LogError(ex); Message = "StorageOperationError".GetLocalizedResource() + Environment.NewLine + ex.Message; }
        finally { IsBusy = false; }
    }

    // Создаём согласованную резервную копию и обновляем список
    private async Task CreateBackupAsync()
    {
        await _inspector.BackupAsync(SettingsService.AppStorage);
        await RefreshSnapshotAsync();
        Message = "StorageBackupCreated".GetLocalizedResource();
    }

    // Удаляем подтверждённую копию и обновляем сведения паспорта
    private Task DeleteBackupAsync() => RunAsync(async () =>
    {
        var backup = SelectedBackup;
        if (backup is null) return;
        if (!await ConfirmAsync("StorageDeleteBackupTitle", "StorageDeleteBackupHint", backup.Name)) return;
        await _inspector.DeleteBackupAsync(SettingsService.AppStorage, backup.Path);
        await RefreshSnapshotAsync();
        Message = "StorageBackupDeleted".GetLocalizedResource();
    });

    // Выводим результат диагностики без изменения базы
    private async Task CheckAsync()
    {
        var result = await _inspector.CheckAsync(SettingsService.AppStorage);
        Message = result == "ok" ? "StorageIntegrityOk".GetLocalizedResource() : result;
    }

    // Не обслуживаем базу во время сетевых загрузок или другой работы с хранилищем
    private void EnsureMaintenanceAllowed()
    {
        if (LoadStateService.IsStorageProcessing || LoadStateService.NetworkActivity.Operations.Any(x => x.State == TgNetworkOperationState.Running))
            throw new InvalidOperationException("StorageStopOperations".GetLocalizedResource());
    }

    // Подтверждение и резервное копирование выполняются до изменения базы
    private Task MaintainAsync(string title, Func<Task> action) => RunAsync(async () =>
    {
        EnsureMaintenanceAllowed();
        if (!await ConfirmAsync(title, "StorageMaintenanceHint")) return;
        EnsureMaintenanceAllowed();
        if (BackupBeforeMaintenance) await _inspector.BackupAsync(SettingsService.AppStorage);
        await action();
        await RefreshSnapshotAsync();
        Message = "StorageMaintenanceDone".GetLocalizedResource();
    });

    // Восстанавливаем в отдельный файл и применяем его штатной сменой профиля
    private Task RestoreAsync() => RunAsync(async () =>
    {
        if (SelectedBackup is null) return;
        EnsureMaintenanceAllowed();
        if (!await ConfirmAsync("StorageRestoreTitle", "StorageRestoreExplanation")) return;
        EnsureMaintenanceAllowed();
        var path = await _inspector.RestoreAsync(SettingsService.AppStorage, SelectedBackup.Path);
        var previous = SettingsService.ReadSetting<TgAccountProfiles.State>(TgAccountProfiles.SettingsKey)
            ?? throw new InvalidOperationException("StorageProfileMissing".GetLocalizedResource());
        var updated = previous with { Profiles = previous.Profiles.Select(x => x.Id == previous.ActiveProfileId ? x with { StoragePath = path } : x).ToList() };
        await App.RestartForProfileAsync(() => SettingsService.SaveProfileState(updated), () => SettingsService.SaveProfileState(previous));
    });

    // Запрашиваем подтверждение до выполнения длительной операции
    private async Task<bool> ConfirmAsync(string title, string hint, string? fileName = null)
    {
        if (XamlRootVm is null) return false;
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRootVm, Title = title.GetLocalizedResource(),
            Content = fileName is null ? hint.GetLocalizedResource() : string.Format(CultureInfo.CurrentCulture, hint.GetLocalizedResource(), fileName),
            PrimaryButtonText = TgResourceExtensions.GetYesButton(), CloseButtonText = TgResourceExtensions.GetCancelButton(),
            DefaultButton = ContentDialogButton.Close,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    // Получаем метрику по имени таблицы
    private string Metric(string table) => IsDisplaySensitiveData
        ? (_snapshot?.Tables.FirstOrDefault(x => x.Name == table)?.Count ?? 0).ToString("N0") : SensitiveData;
    /// <summary> Форматирует размер файла </summary>
    public static string Size(long bytes) => TgFileUtils.GetFileSizeAsString(bytes);

    /// <summary> Локализованная строка каталога таблиц </summary>
    public sealed class StorageTable(string name, long count, bool visible) : ObservableObject
    {
        private bool _visible = visible;
        public string Name { get; } = name;
        public long Count { get; } = count;
        public string Title => ("StorageTable_" + Name).GetLocalizedResource();
        public string CountText => _visible ? Count.ToString("N0") : "•••";
        /// <summary> Обновляет видимость счётчика без повторного чтения базы </summary>
        public void SetVisibility(bool value) { _visible = value; OnPropertyChanged(nameof(CountText)); }
        /// <summary> Обновляет перевод и формат счётчика </summary>
        public void RefreshLabels() { OnPropertyChanged(nameof(Title)); OnPropertyChanged(nameof(CountText)); }
    }
    /// <summary> Краткая строка и полная карточка записи </summary>
    public sealed record StorageRecord(string Title, string Id, IReadOnlyList<StorageField> Fields);
    /// <summary> Поле записи без секретных столбцов </summary>
    public sealed record StorageField(string Name, string Value);
    /// <summary> Представление резервной копии </summary>
    public sealed record StorageBackup(string Path, long Bytes, DateTime Created)
    {
        public string Name => System.IO.Path.GetFileName(Path);
        public string Description => $"{Created:g} · {Size(Bytes)}";
    }
}
