namespace OpenTgResearcherDesktop.ViewModels;

// Подменяем только зависимости оболочки, команды верхней панели компилируем из исходного partial-файла
public partial class ShellViewModel : ObservableObject
{
    public ITgSettingsService SettingsService { get; } = new();
    public ILoadStateService LoadStateService { get; } = new LoadStateService();
    public bool IsFloodVisible { get; set; }
    public int FloodWaitSeconds { get; set; }
    public INavigationService NavigationService { get; } = new();
    public TgUpdateViewModel Updates { get; } = new(new LoadStateService(), new(), new(), null!);
}

public sealed class XamlRoot;
public sealed class TgClientConnectionViewModel;
