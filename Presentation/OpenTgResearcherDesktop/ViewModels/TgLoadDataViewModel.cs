// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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