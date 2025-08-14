// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgDownloadViewModel : ITgDebug
{
    public TgDownloadChat Chat { get; set; }
	public int SourceScanCount { get; set; }
	public int SourceScanCurrent { get; set; }
	public bool IsSaveMessages { get; set; }
}
