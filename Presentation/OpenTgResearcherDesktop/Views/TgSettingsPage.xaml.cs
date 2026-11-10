namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgSettingsPage
{
	#region Fields, properties, constructor

	public override TgSettingsViewModel ViewModel { get; }

	public TgSettingsPage()
	{
		ViewModel = App.GetService<TgSettingsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
		Loaded += PageLoaded;
	}

	#endregion

    // Синхронизируем палитру с введённым значением при открытии выбора цвета
    private void AccentFlyout_Opening(object sender, object e)
    {
        try { AccentPicker.Color = TgProgramIntegration.ParseAccent(ViewModel.Preferences.AccentHex); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Сохраняем выбранный цвет в черновике параметров до нажатия кнопки сохранения
    private void AccentPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args) =>
        ViewModel.Preferences.AccentHex = $"#{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
}
