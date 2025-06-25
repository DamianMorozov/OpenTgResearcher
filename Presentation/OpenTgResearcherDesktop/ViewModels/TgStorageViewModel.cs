// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using CommunityToolkit.WinUI.UI.Controls.TextToolbarButtons;

using System.Threading.Tasks;

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgStorageTableDto> StorageTableDtos { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgStorageBackupDto> StorageBackupDtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsStorageLoadDataShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageSetupShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageAdvancedSettingShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageClearingShow { get; set; }
    [ObservableProperty]
    public partial string StorageLog { get; set; }

    public IRelayCommand StorageSetupCommand { get; }
    public IRelayCommand StorageCreateBackupCommand { get; }
    public IRelayCommand StorageShrinkCommand { get; }
    public IRelayCommand StorageClear { get; }
    public IRelayCommand StorageAdvancedSettingCommand { get; }
    public IRelayCommand StorageClearingCommand { get; }
    public IRelayCommand StorageChatsCommand { get; }
    public IRelayCommand StorageFiltersCommand { get; }

    public TgStorageViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgStorageViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgStorageViewModel))
    {
        StorageLog = string.Empty;
        // Commands
        StorageSetupCommand = new AsyncRelayCommand(StorageSetupAsync);
        StorageCreateBackupCommand = new AsyncRelayCommand(StorageCreateBackupAsync);
        StorageShrinkCommand = new AsyncRelayCommand(StorageShrinkAsync);
        StorageClear = new AsyncRelayCommand(StorageClearAsync);
        StorageAdvancedSettingCommand = new AsyncRelayCommand(StorageAdvancedSettingsAsync);
        StorageClearingCommand = new AsyncRelayCommand(StorageClearingAsync);
        StorageChatsCommand = new AsyncRelayCommand(StorageChatsAsync);
        StorageFiltersCommand = new AsyncRelayCommand(StorageFiltersAsync);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            await LoadStorageTableDtosAsync();
            await ReloadUiAsync();
        });

    /// <summary> Load storage table dtos </summary>
    private async Task LoadStorageTableDtosAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        try
        {
            StorageTableDtos = await App.BusinessLogicManager.LoadStorageTableDtosAsync(TgResourceExtensions.GetTableNameApps(),
                TgResourceExtensions.GetTableNameChats(), TgResourceExtensions.GetTableNameContacts(), TgResourceExtensions.GetTableNameDocuments(),
                TgResourceExtensions.GetTableNameFilters(), TgResourceExtensions.GetTableNameMessages(), TgResourceExtensions.GetTableNameProxies(),
                TgResourceExtensions.GetTableNameStories(), TgResourceExtensions.GetTableNameVersions());
        }
        finally
        {
            IsStorageLoadDataShow = true;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = false;
        }
    }

    /// <summary> Setup storage </summary>
    private async Task StorageSetupAsync()
    {
        try
        {
            await LoadStorageBackupDtosAsync();
        }
        finally
        {
            StorageLog = string.Empty;
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = true;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = false;
        }
    }

    /// <summary> Load storage backup dtos </summary>
    private async Task LoadStorageBackupDtosAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        StorageBackupDtos = App.BusinessLogicManager.LoadStorageBackupDtos(SettingsService.AppStorage);
        await Task.CompletedTask;
    }

    /// <summary> Backup storage </summary>
    private async Task StorageCreateBackupAsync() => await ContentDialogAsync(StorageCreateBackupCoreAsync,
        TgResourceExtensions.AskActionStorageBackup());

    private async Task StorageCreateBackupCoreAsync()
    {
        try
        {
            StorageLog = string.Empty;
            var backupResult = App.BusinessLogicManager.BackupDb(SettingsService.AppStorage);
            StorageLog = $"{TgResourceExtensions.ActionStorageCreateBackupFile()}: {backupResult.FileName}";
            StorageLog = backupResult.IsSuccess
                ? TgResourceExtensions.ActionStorageCreateBackupSuccess() : TgResourceExtensions.ActionStorageCreateBackupFailed();
        }
        finally
        {
            await LoadStorageBackupDtosAsync();
        }
    }

    /// <summary> Shrink storage </summary>
    private async Task StorageShrinkAsync() => await ContentDialogAsync(StorageShrinkCoreAsync,
        TgResourceExtensions.AskActionStorageShrink());

    private async Task StorageShrinkCoreAsync()
    {
        try
        {
            StorageLog = string.Empty;
            await App.BusinessLogicManager.ShrinkDbAsync();
            await LoadStorageBackupDtosAsync();
        }
        finally
        {
            await Task.Delay(250);
        }
    }

    /// <summary> Clear storage </summary>
    private async Task StorageClearAsync()
    {
        await ContentDialogAsync(StorageClearCoreAsync, TgResourceExtensions.AskActionStorageClear());

        ContentDialog dialog = new()
        {
            XamlRoot = XamlRootVm,
            Title = TgResourceExtensions.GetManualDeleteFile(SettingsService.AppStorage),
            CloseButtonText = TgResourceExtensions.GetOkButton(),
            DefaultButton = ContentDialogButton.Close,
        };
        _ = await dialog.ShowAsync();
    }

    private async Task StorageClearCoreAsync()
    {
        try
        {
            StorageLog = TgResourceExtensions.GetManualDeleteFile(SettingsService.AppStorage);
        }
        finally
        {
            await Task.Delay(250);
        }
    }

    /// <summary> Advanced settings </summary>
    private async Task StorageAdvancedSettingsAsync()
    {
        try
        {
            await Task.CompletedTask;
        }
        finally
        {
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = true;
            IsStorageClearingShow = false;
        }
    }

    // <summary> Storage clearing </summary>
    private async Task StorageClearingAsync()
    {
        try
        {
            await Task.CompletedTask;
        }
        finally
        {
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = true;
        }
    }

    private async Task StorageChatsAsync()
    {
        await Task.CompletedTask;
    }

    private async Task StorageFiltersAsync()
    {
        await Task.CompletedTask;
    }

    #endregion
}