namespace TgBusinessLogic.Contracts;

public interface ITgDownloadViewModel : ITgDebug
{
    public TgDownloadChat Chat { get; set; }
	public int SourceScanCount { get; set; }
	public int SourceScanCurrent { get; set; }
}
