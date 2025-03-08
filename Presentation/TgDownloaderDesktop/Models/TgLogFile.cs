// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Models;

public sealed class TgLogFile(ITgLogsViewModel viewModel, string fileName, string content)
{
	public ITgLogsViewModel ViewModel { get; set; } = viewModel;
	public string FileName { get; set; } = fileName;
	public string Content { get; set; } = content;
}