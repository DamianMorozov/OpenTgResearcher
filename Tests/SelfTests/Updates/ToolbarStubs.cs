namespace OpenTgResearcherDesktop.ViewModels;

// Подменяем только зависимости оболочки, команды верхней панели компилируем из исходного partial-файла
public partial class ShellViewModel : ObservableObject
{
    public ITgSettingsService SettingsService { get; } = new();
    public ILoadStateService LoadStateService { get; } = new LoadStateService();
    public bool IsFloodVisible { get; set; }
    public int FloodWaitSeconds { get; set; }
    public INavigationService NavigationService { get; } = new();
    public TgSettingsViewModel Settings { get; }
    public TgUpdateViewModel Updates => Settings.Updates;

    /// <summary> Связывает верхнюю панель и настройки с одной моделью обновлений </summary>
    public ShellViewModel()
    {
        var updates = new TgUpdateViewModel(LoadStateService, SettingsService, NavigationService, null!);
        Settings = new TgSettingsViewModel(LoadStateService, SettingsService, NavigationService, null!, updates);
    }
}

public sealed class XamlRoot;
public sealed class TgClientConnectionViewModel;
