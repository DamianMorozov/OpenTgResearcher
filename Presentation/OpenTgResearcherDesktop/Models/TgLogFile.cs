namespace OpenTgResearcherDesktop.Models;

public sealed class TgLogFile(ITgLogsViewModel viewModel, string fileName, string fileSize, string content)
{
	public ITgLogsViewModel ViewModel { get; set; } = viewModel;
	public string FileName { get; set; } = fileName;
	public string FileSize { get; set; } = fileSize;
	public string Content { get; set; } = content;
}
