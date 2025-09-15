namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgMainViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgMainViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, logger, nameof(TgMainViewModel))
{
	#region Fields, properties, constructor

    //

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

	#endregion
}
