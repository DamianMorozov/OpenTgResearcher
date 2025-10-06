namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageAdvancedViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string StorageLog { get; set; }

    public IAsyncRelayCommand StorageClear { get; }
    public IAsyncRelayCommand StorageResetAutoUpdateCommand { get; }

    public TgStorageAdvancedViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgStorageAdvancedViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStorageAdvancedViewModel))
    {
        StorageLog = string.Empty;
        // Commands
        StorageClear = new AsyncRelayCommand(StorageClearAsync);
        StorageResetAutoUpdateCommand = new AsyncRelayCommand(StorageResetAutoUpdateAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

    /// <summary> Clear storage </summary>
    private async Task StorageClearAsync()
    {
        await ContentDialogAsync(StorageClearCoreAsync, TgResourceExtensions.AskActionStorageClear(), TgEnumLoadDesktopType.Storage);

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

    /// <summary> Clear storage </summary>
    private async Task StorageResetAutoUpdateAsync() =>
        await ContentDialogAsync(StorageResetAutoUpdateCoreAsync, TgResourceExtensions.AskActionStorageResetAutoUpdate(), TgEnumLoadDesktopType.Storage);

    /// <summary> Reset auto update field </summary>
    private async Task StorageResetAutoUpdateCoreAsync()
    {
        try
        {
            StorageLog = string.Empty;
            await App.BusinessLogicManager.StorageManager.SourceRepository.ResetAutoUpdateAsync();
            StorageLog = TgResourceExtensions.ActionStorageResetAutoUpdateSuccess();
        }
        catch (Exception ex)
        {
            StorageLog = TgResourceExtensions.ActionStorageResetAutoUpdateSuccess();
            StorageLog += Environment.NewLine + ex.Message;
            if (ex.InnerException is not null)
                StorageLog += Environment.NewLine + ex.InnerException.Message;
        }
        finally
        {
            await Task.Delay(250);
        }
    }

    #endregion
}
