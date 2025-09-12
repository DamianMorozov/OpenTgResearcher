namespace OpenTgResearcherDesktop.Contracts.ViewModels;

public interface ITgLogsViewModel : ITgPageViewModel
{
	public IRelayCommand DeleteLogFileCommand { get; }
}
