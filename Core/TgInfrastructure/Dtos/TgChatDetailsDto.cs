// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsDto : TgSensitiveDto
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Type { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Id { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string InviteLink { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Permissions { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsChatFull { get; set; }
    [ObservableProperty]
    public partial string About { get; set; } = "-";
    [ObservableProperty]
    public partial int ParticipantsCount { get; set; }
    [ObservableProperty]
    public partial int OnlineCount { get; set; }
    [ObservableProperty]
    public partial string SlowMode { get; set; } = "-";
    [ObservableProperty]
    public partial string AvailableReactions { get; set; } = "-";
    [ObservableProperty]
    public partial int TtlPeriod { get; set; }
    [ObservableProperty]
    public partial bool IsActiveChat { get; set; }
    [ObservableProperty]
    public partial bool IsBanned { get; set; }
    [ObservableProperty]
    public partial bool IsChannel { get; set; }
    [ObservableProperty]
    public partial bool IsGroup { get; set; }
    [ObservableProperty]
    public partial bool IsForum { get; set; }

    #endregion

    #region Public and private methods

    //

    #endregion
}
