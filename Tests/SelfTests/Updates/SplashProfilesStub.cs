namespace OpenTgResearcherDesktop.ViewModels;

// Проверяем исходную логику профилей заставки без мониторинга оборудования и запуска Telegram
public partial class TgSplashScreenViewModel : TgPageViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChooseProfile))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    public partial bool IsLoadComplete { get; set; }
    public IAsyncRelayCommand ContinueCommand { get; }
    public Action BackToMainWindow { get; set; } = () => { };
    public int SettingsLoads { get; private set; }

    public TgSplashScreenViewModel(ITgSettingsService settings) : base(new LoadStateService(), settings, new(), null!, "Splash")
    {
        InitializeSplashProfiles();
        ContinueCommand = new AsyncRelayCommand(ContinueAsync, () => CanChooseProfile);
    }

    // Учитываем продолжение загрузки оформления без доступа к WinUI
    private Task LoadingSettingsAsync() { SettingsLoads++; return Task.CompletedTask; }
}
