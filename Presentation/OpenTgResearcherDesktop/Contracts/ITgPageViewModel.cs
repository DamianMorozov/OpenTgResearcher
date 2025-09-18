namespace OpenTgResearcherDesktop.Contracts;

public interface ITgPageViewModel
{
    public ILoadStateService LoadStateService { get; }
    public string Name { get; }
    public Task OnNavigatedToAsync(NavigationEventArgs? e);
    public void OnLoaded(object parameter);
    public Task ReloadUiAsync();

    public void LogInformation(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");
    public void LogDebug(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");
    public void LogWarning(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");
    public void LogError(Exception ex, string message = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");
}
