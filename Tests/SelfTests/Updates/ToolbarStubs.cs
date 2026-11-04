namespace OpenTgResearcherDesktop.ViewModels;

// Подменяем только зависимости оболочки, команды верхней панели компилируем из исходного partial-файла
public partial class ShellViewModel : ObservableObject
{
    public ITgSettingsService SettingsService { get; } = new();
    public INavigationService NavigationService { get; } = new();
    public TgUpdateViewModel Updates { get; } = new(new LoadStateService(), new(), new(), null!);
}

public sealed class XamlRoot;
