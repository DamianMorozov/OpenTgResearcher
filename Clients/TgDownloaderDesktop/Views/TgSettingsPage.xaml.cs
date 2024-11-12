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
		Loaded += OnLoaded;
	}

	#endregion

	#region Public and private methods

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		await ViewModel.SettingsService.LoadAsync();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		ViewModel.OnLoaded(XamlRoot);
		ComboBoxAppThemes.SelectionChanged += Selector_OnSelectionChanged;
	}

	private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.SetThemeAsync();

	#endregion
}
