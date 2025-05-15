// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgMainViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgMainViewModel> logger)
	: TgPageViewModelBase(settingsService, navigationService, licenseService, logger, nameof(TgMainViewModel))
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string DonateUsdtTrc20 { get; set; } = "TBTDRWnMBw7acfpkhAXjSQNSNHQGFR662Y";
	[ObservableProperty]
	public partial string DonateUsdtTon { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateToncoin { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateBitcoin { get; set; } = "1FJayytWUK6vkxK2nUcD2TJskk3g9ZnmfW";
	[ObservableProperty]
	public partial string DonateNotcoin { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateDogs { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateHmstr { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateX { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateCatizen { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";
	[ObservableProperty]
	public partial string DonateMajor { get; set; } = "UQBkjSs3XPmraI_sS4Mf05SMd1y44DahNhwPg9ySp3V-M3N6";

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();
	});

	#endregion
}
