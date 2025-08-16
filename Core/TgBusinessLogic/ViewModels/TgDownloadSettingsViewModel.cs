// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.ViewModels;

/// <summary> Download settings </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgDownloadSettingsViewModel : ObservableRecipient, ITgDownloadViewModel
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial TgEfUserViewModel ContactVm { get; set; }
	[ObservableProperty]
	public partial TgEfSourceViewModel SourceVm { get; set; }
	[ObservableProperty]
	public partial TgEfStoryViewModel StoryVm { get; set; }
	[ObservableProperty]
	public partial TgEfVersionViewModel VersionVm { get; set; }
	[ObservableProperty] 
	public partial bool IsSaveMessages { get; set; } = true;
	[ObservableProperty]
	public partial bool IsRewriteFiles { get; set; }
	[ObservableProperty]
	public partial bool IsRewriteMessages { get; set; }
	[ObservableProperty]
	public partial bool IsJoinFileNameWithMessageId { get; set; } = true;
	[ObservableProperty]
	public partial int CountThreads { get; set; } = 5;
	[ObservableProperty]
	public partial int LimitThreads { get; set; } = 10;
	[ObservableProperty]
	public partial TgDownloadChat Chat { get; set; }
	[ObservableProperty]
	public partial int SourceScanCount { get; set; } = 1;
	[ObservableProperty]
	public partial int SourceScanCurrent { get; set; } = 1;

	public TgDownloadSettingsViewModel()
	{
		ContactVm = new(TgGlobalTools.Container);
		SourceVm = new(TgGlobalTools.Container);
		StoryVm = new(TgGlobalTools.Container);
		VersionVm = new(TgGlobalTools.Container);
		Chat = new();
	}

	#endregion

	#region Public and private methods

    public string ToDebugString() => $"{SourceVm.ToDebugString()}";

    #endregion
}