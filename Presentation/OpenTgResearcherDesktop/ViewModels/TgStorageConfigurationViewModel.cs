namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageConfigurationViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgStorageBackupDto> StorageBackupDtos { get; set; } = [];
    [ObservableProperty]
    public partial string StorageLog { get; set; }

    public IAsyncRelayCommand StorageCreateBackupCommand { get; }
    public IAsyncRelayCommand StorageShrinkCommand { get; }
    public IAsyncRelayCommand StorageClear { get; }

    public TgStorageConfigurationViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgStorageConfigurationViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgStorageConfigurationViewModel))
    {
        StorageLog = string.Empty;
        // Commands
        StorageCreateBackupCommand = new AsyncRelayCommand(StorageCreateBackupAsync);
        StorageShrinkCommand = new AsyncRelayCommand(StorageShrinkAsync);
        StorageClear = new AsyncRelayCommand(StorageClearAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        await LoadStorageBackupDtosAsync();
        await ReloadUiAsync();
    });

    /// <summary> Load storage backup dtos </summary>
    private async Task LoadStorageBackupDtosAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        StorageBackupDtos = App.BusinessLogicManager.StorageManager.LoadStorageBackupDtos(SettingsService.AppStorage);
        await Task.CompletedTask;
    }

    /// <summary> Backup storage </summary>
    private async Task StorageCreateBackupAsync() => 
        await ContentDialogAsync(StorageCreateBackupCoreAsync, TgResourceExtensions.AskActionStorageBackup(), TgEnumLoadDesktopType.Storage);

    private async Task StorageCreateBackupCoreAsync()
    {
        try
        {
            StorageLog = string.Empty;
            var (IsSuccess, FileName) = App.BusinessLogicManager.StorageManager.BackupDb(SettingsService.AppStorage);
            StorageLog += $"{TgResourceExtensions.ActionStorageCreateBackupFile()}: {FileName}";
            StorageLog += IsSuccess ? TgResourceExtensions.ActionStorageCreateBackupSuccess() : TgResourceExtensions.ActionStorageCreateBackupFailed();
        }
        finally
        {
            await LoadStorageBackupDtosAsync();
        }
    }

    /// <summary> Shrink storage </summary>
    private async Task StorageShrinkAsync() => 
        await ContentDialogAsync(StorageShrinkCoreAsync, TgResourceExtensions.AskActionStorageShrink(), TgEnumLoadDesktopType.Storage);

    private async Task StorageShrinkCoreAsync()
    {
        try
        {
            await App.BusinessLogicManager.StorageManager.ShrinkDbAsync();
        }
        finally
        {
            await LoadStorageBackupDtosAsync();
        }
    }

    /// <summary> Clear storage </summary>
    private async Task StorageClearAsync() => 
        await ContentDialogAsync(StorageClearCoreAsync, TgResourceExtensions.AskActionStorageClear(), TgEnumLoadDesktopType.Storage);

    private async Task StorageClearCoreAsync()
    {
        try
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRootVm,
                Title = TgResourceExtensions.GetManualDeleteFile(SettingsService.AppStorage),
                CloseButtonText = TgResourceExtensions.GetOkButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            _ = await dialog.ShowAsync();
        }
        finally
        {
            StorageLog = TgResourceExtensions.GetManualDeleteFile(SettingsService.AppStorage);
        }
    }

    #endregion
}
