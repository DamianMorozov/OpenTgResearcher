﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public sealed partial class TgSettingsPage : Page
{
	#region Public and private fields, properties, constructor

	public TgSettingsViewModel ViewModel { get; }

	public TgSettingsPage()
	{
		ViewModel = App.GetService<TgSettingsViewModel>();
		InitializeComponent();
		ComboBoxAppThemes.SelectionChanged += ComboBoxAppThemes_OnSelectionChanged;
		ComboBoxAppLanguages.SelectionChanged += ComboBoxAppLanguages_OnSelectionChanged;
		Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		await ViewModel.OnNavigatedToAsync(e);
	}

	private void PageLoaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded(XamlRoot);

	private async void ComboBoxAppThemes_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.SettingsService.SetAppThemeAsync();

	private async void ComboBoxAppLanguages_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.SettingsService.SetAppLanguageAsync();

	#endregion
}
