namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgLoadDataViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService,
    ILogger<TgLoadDataViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, loadStateService, logger, nameof(TgLoadDataViewModel))
{
    #region Fields, properties, constructor

    //

    #endregion
}
