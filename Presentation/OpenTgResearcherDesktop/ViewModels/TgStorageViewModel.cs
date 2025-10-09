namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgStorageViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Frame ContentFrame { get; set; } = default!;

    public TgStorageViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgStorageViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStorageViewModel))
    {
        //
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

    public override async Task ReloadUiAsync()
    {
        await base.ReloadUiAsync();

        if (ContentFrame is not null)
        {
            var viewModel = await ContentFrame.GetPageViewModelAsync();
            if (viewModel is TgPageViewModelBase pageViewModelBase)
            {
                await LoadStorageDataAsync(pageViewModelBase.ReloadUiAsync);
            }
        }
    }

    #endregion
}
