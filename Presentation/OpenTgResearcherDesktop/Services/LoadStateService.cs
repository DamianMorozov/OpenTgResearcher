namespace OpenTgResearcherDesktop.Services;

/// <summary> Load state service to track loading states in the application </summary>
public sealed partial class LoadStateService : ObservableObject, ILoadStateService
{
    [ObservableProperty]
    public partial bool IsStorageProcessing { get; set; }
    [ObservableProperty]
    public partial bool IsOnlineProcessing { get; set; }
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; set; }
    public string SensitiveData => "**********";
}
