namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgLoadDataViewModel(ITgSettingsService settingsService, INavigationService navigationService, 
	ILogger<TgLoadDataViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, logger, nameof(TgLoadDataViewModel))
{
    #region Fields, properties, constructor

	//

    #endregion
}
