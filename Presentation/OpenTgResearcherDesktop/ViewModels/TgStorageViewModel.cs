namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Frame ContentFrame { get; set; } = default!;

    public TgStorageViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgStorageViewModel> logger) : base(settingsService, navigationService, loadStateService, logger, nameof(TgStorageViewModel))
    {
        //
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

    protected override async Task SetDisplaySensitiveAsync()
    {
        if (ContentFrame.GetPageViewModel() is TgPageViewModelBase pageViewModelBase)
        {
            pageViewModelBase.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await pageViewModelBase.ReloadUiAsync();
        }

        await Task.CompletedTask;
    }

    public override async Task ReloadUiAsync()
    {
        await base.ReloadUiAsync();

        if (ContentFrame is not null && ContentFrame.GetPageViewModel() is TgPageViewModelBase pageViewModelBase)
        {
            await LoadStorageDataAsync(pageViewModelBase.ReloadUiAsync);
        }
    }

    #endregion
}
