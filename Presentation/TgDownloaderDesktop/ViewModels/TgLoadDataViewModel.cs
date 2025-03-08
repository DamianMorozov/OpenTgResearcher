// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgLoadDataViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

	public TgLoadDataViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgLoadDataViewModel> logger) 
		: base(settingsService, navigationService, logger)
	{
	}

	#endregion
}