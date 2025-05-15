// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgLoadDataViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, 
	ILogger<TgLoadDataViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, licenseService, logger, nameof(TgLoadDataViewModel))
{
    #region Public and private fields, properties, constructor

	//

    #endregion
}