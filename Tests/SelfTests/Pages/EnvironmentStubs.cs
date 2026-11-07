namespace OpenTgResearcherDesktop.Common;

// Подменяем только окружение WinUI, исходные обёртки загрузки используют настоящую службу состояния
public abstract partial class TgPageViewModelBase : ObservableObject
{
    public ILoadStateService LoadStateService { get; init; } = null!;
    public List<Exception> Errors { get; } = [];
    public int LicenseRefreshes { get; private set; }
    /// <summary> Сохраняет ошибку исходного обработчика загрузки </summary>
    public void LogError(Exception ex, string message) => Errors.Add(ex);
    /// <summary> Учитывает завершение полного обработчика страницы </summary>
    public void RefreshLicenseInfo() => LicenseRefreshes++;
}
