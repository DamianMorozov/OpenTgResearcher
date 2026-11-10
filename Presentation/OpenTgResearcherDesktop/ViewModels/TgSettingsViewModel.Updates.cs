namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSettingsViewModel
{
    [ObservableProperty]
    public partial bool IsUpdatesExpanded { get; set; }

    public event EventHandler? UpdatesRequested;

    /// <summary> Раскрывает обновления и запрашивает прокрутку без сброса черновиков или текущей загрузки </summary>
    public void ShowUpdates()
    {
        IsUpdatesExpanded = true;
        UpdatesRequested?.Invoke(this, EventArgs.Empty);
    }
}
