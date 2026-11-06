namespace TgBusinessLogic.ViewModels;

/// <summary> Download settings </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgDownloadSettingsViewModel : ObservableRecipient, ITgDownloadViewModel
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial TgEfUserViewModel ContactVm { get; set; }
	[ObservableProperty]
	public partial TgEfSourceViewModel SourceVm { get; set; }
	[ObservableProperty]
	public partial TgEfStoryViewModel StoryVm { get; set; }
	[ObservableProperty]
	public partial TgEfVersionViewModel VersionVm { get; set; }
    [ObservableProperty]
	public partial TgDownloadChat Chat { get; set; }
	[ObservableProperty]
	public partial int SourceScanCount { get; set; } = 1;
	[ObservableProperty]
	public partial int SourceScanCurrent { get; set; } = 1;
    
    public int LimitThreads => TgGlobalTools.DownloadCountThreadsLimit;

	public TgDownloadSettingsViewModel()
	{
		ContactVm = new(TgGlobalTools.Container);
		SourceVm = new(TgGlobalTools.Container);
		StoryVm = new(TgGlobalTools.Container);
		VersionVm = new(TgGlobalTools.Container);
		Chat = new();
	}

	#endregion

	#region Methods

    public string ToDebugString() => $"{SourceVm.ToDebugString()}";

    /// <summary> Создаёт независимые настройки запуска, не привязанные к редактируемой странице чата </summary>
    public TgDownloadSettingsViewModel CreateParsingSnapshot()
    {
        var snapshot = new TgDownloadSettingsViewModel { SourceScanCount = SourceScanCount, SourceScanCurrent = SourceScanCurrent };
        snapshot.SourceVm.Dto = (TgEfSourceDto)TgEfDomainUtils.CreateNewDto(SourceVm.Dto, isUidCopy: true);
        snapshot.Chat.Base = Chat.Base;
        return snapshot;
    }

    #endregion
}
