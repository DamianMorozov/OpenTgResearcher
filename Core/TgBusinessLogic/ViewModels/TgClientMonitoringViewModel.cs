// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgInfrastructure.Dtos;

public sealed partial class TgClientMonitoringViewModel : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial List<string> ChatNames { get; set; } = [];
    [ObservableProperty]
    public partial List<long> ChatIds { get; set; } = [];
    [ObservableProperty]
    public partial List<string> Keywords { get; set; } = [];
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial long UserId { get; set; }
    [ObservableProperty]
    public partial bool IsStartMonitoring { get; set; }
    [ObservableProperty]
    public partial bool IsStartSearching { get; set; }
    [ObservableProperty]
    public partial bool IsSendMessages { get; set; }
    [ObservableProperty]
    public partial bool IsSendToMyself { get; set; }
    [ObservableProperty]
    public partial bool IsSearchAtAllChats { get; set; }
    [ObservableProperty]
    public partial bool IsSkipKeywords { get; set; }
    [ObservableProperty]
    public partial Contacts_ResolvedPeer? ResolvedPeer { get; set; }
    [ObservableProperty]
    public partial string LastUserLink { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastDateTime { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastMessageText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastMessageLink { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int CatchMessages { get; set; }

    #endregion

    #region Methods

    public void Default()
    {
        UserName = string.Empty;
        UserId = 0;
        ChatNames.Clear();
        ChatIds.Clear();
        Keywords.Clear();
        IsSendToMyself = false;
        IsSearchAtAllChats = false;
        IsSkipKeywords = false;
        ResolvedPeer = null;
        LastUserLink = string.Empty;
        LastDateTime = string.Empty;
        LastMessageText = string.Empty;
        LastMessageLink = string.Empty;
        CatchMessages = 0;
    }

    #endregion
}
