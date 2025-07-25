﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgMainViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgMainViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, logger, nameof(TgMainViewModel))
{
	#region Public and private fields, properties, constructor

    //

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();
	});

	#endregion
}
