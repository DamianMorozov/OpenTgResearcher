namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgMainViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
    ILogger<TgMainViewModel> logger) : TgPageViewModelBase(loadStateService, settingsService, navigationService, logger, nameof(TgMainViewModel))
{
	#region Fields, properties, constructor

    //

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

	#endregion
}
