// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgSettingsViewModel : TgPageViewModelBase, INavigationAware
{
	#region Public and private fields, properties, constructor

	public Wpf.Ui.Appearance.ThemeType CurrentTheme { get; set; } = Wpf.Ui.Appearance.ThemeType.Unknown;
	public Brush TextBrush { get; set; } = Brushes.White;
	public Color TextColor { get; set; } = Color.White;

	public void OnNavigatedTo()
    {
        var task = InitializeViewModelAsync();
		task.Wait();
	}

    #endregion

    #region Public and private methods

    public void OnNavigatedFrom() { }

    protected override async Task InitializeViewModelAsync()
    {
        await base.InitializeViewModelAsync();

        CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
	}

    // ChangeThemeCommand
	[RelayCommand]
    public async Task OnChangeThemeAsync(string parameter)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                        break;
                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;
                    TextBrush = Brushes.Black;
                    TextColor = Color.Black;
					break;
                default:
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                        break;
                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;
                    TextBrush = Brushes.White;
                    TextColor = Color.White;
					break;
            }

        }, false).ConfigureAwait(false);
    }

    #endregion
}