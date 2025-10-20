using System.ComponentModel;

namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSettingsViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

    public IAsyncRelayCommand SettingsLoadCommand { get; }
    public IAsyncRelayCommand SettingsDefaultCommand { get; }
    public IAsyncRelayCommand SettingsSaveCommand { get; }

    public TgSettingsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgSettingsViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgSettingsViewModel))
    {
        // Commands
        SettingsLoadCommand = new AsyncRelayCommand(SettingsLoadAsync);
        SettingsDefaultCommand = new AsyncRelayCommand(SettingsDefaultAsync);
        SettingsSaveCommand = new AsyncRelayCommand(SettingsSaveAsync);
    }


    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        CheckIfDisposed();

        base.ReleaseManagedResources();
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        SettingsService.Load();
        await Task.CompletedTask;
    }

    private async Task SettingsLoadAsync() => 
        await ContentDialogAsync(SettingsService.Load, TgResourceExtensions.AskSettingsLoad(), TgEnumLoadDesktopType.Storage);

    private async Task SettingsDefaultAsync() => 
        await ContentDialogAsync(SettingsDefaultCoreAsync, TgResourceExtensions.AskSettingsDefault(), TgEnumLoadDesktopType.Storage);

    private async Task SettingsDefaultCoreAsync()
    {
        SettingsService.Default();
        await Task.CompletedTask;
    }

    private async Task SettingsSaveAsync() => 
        await ContentDialogAsync(SettingsSaveCore, TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Storage);

    private void SettingsSaveCore()
    {
        SettingsService.Save();
        SettingsService.Load();
    }

    #endregion
}
