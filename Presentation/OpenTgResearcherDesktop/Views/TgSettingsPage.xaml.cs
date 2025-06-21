// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgSettingsPage
{
	#region Public and private fields, properties, constructor

	public override TgSettingsViewModel ViewModel { get; }

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

	private void ComboBoxAppThemes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
        Task.Run((Func<Task?>)(async () =>
		{
			try
			{
				await ViewModel.SettingsService.SetAppThemeAsync();
			}
			catch (Exception ex)
			{
                TgLogUtils.WriteExceptionWithMessage(ex, "An error occurred set theme!");
			}
		}));
	}

	private void ComboBoxAppLanguages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
        Task.Run((Func<Task?>)(async () =>
		{
			try
			{
				await ViewModel.SettingsService.SetAppLanguageAsync();
			}
			catch (Exception ex)
			{
                TgLogUtils.WriteExceptionWithMessage(ex, "An error occurred set language!");
			}
		}));
	}

	#endregion
}
