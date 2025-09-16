namespace OpenTgResearcherDesktop.Contracts.ViewModels;

public interface ITgLogsViewModel : ITgPageViewModel
{
	public IAsyncRelayCommand DeleteLogFileCommand { get; }
}
