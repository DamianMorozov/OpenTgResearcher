namespace OpenTgResearcherDesktop.Models;

/// <summary> Строка выдачи Telegram с локализованным типом и состоянием сохранения </summary>
public sealed partial class TgPublicChatSearchRow(TgBusinessLogic.Models.TgPublicChatSearchResult source) : ObservableObject
{
    public long Id => source.Id;
    public string Title => source.Title;
    public string UserName => source.UserName;
    public string Identity => $"@{UserName} · {Id}";
    public bool IsGroup => source.IsGroup;
    public bool IsVerified => source.IsVerified;
    public Uri PublicLink => new($"https://t.me/{UserName}");
    [ObservableProperty]
    public partial byte[]? Photo { get; set; }
    [ObservableProperty]
    public partial string PhotoError { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string About { get; set; } = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MembersText))]
    public partial int? ParticipantsCount { get; set; } = source.ParticipantsCount;
    public string MembersText => ParticipantsCount is { } count
        ? string.Format((IsGroup ? "GlobalProfileMembers" : "GlobalProfileSubscribers").GetLocalizedResource(), count)
        : "GlobalProfileCountUnknown".GetLocalizedResource();
    [ObservableProperty]
    public partial ObservableCollection<string> UserNames { get; set; } = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiscussion))]
    public partial Uri? DiscussionLink { get; set; }
    public bool HasDiscussion => DiscussionLink is not null;
    [ObservableProperty]
    public partial bool IsProfileBusy { get; set; }
    [ObservableProperty]
    public partial string ProfileError { get; set; } = string.Empty;
    public bool IsProfileLoaded { get; set; }
    [ObservableProperty]
    public partial bool IsMediaBusy { get; set; }
    [ObservableProperty]
    public partial string MediaError { get; set; } = string.Empty;
    [ObservableProperty]
    public partial ObservableCollection<TgPublicChatMediaRow> Media { get; set; } = [];
    public bool IsMediaLoaded { get; set; }
    public string Kind => (IsGroup ? "GlobalSearchGroup" : "GlobalSearchChannel").GetLocalizedResource();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaved))]
    public partial Guid? SourceUid { get; set; } = source.SourceUid;
    public bool IsSaved => SourceUid.HasValue;

    /// <summary> Обновляет подпись типа при переключении языка </summary>
    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Kind));
        OnPropertyChanged(nameof(MembersText));
        foreach (var item in Media) item.RefreshLanguage();
    }
}

/// <summary> Локализованная строка счётчика Telegram без подмены отсутствующего значения нулём </summary>
public sealed class TgPublicChatMediaRow(TgBusinessLogic.Models.TgPublicChatMediaCount source) : ObservableObject
{
    public string Caption => ("GlobalProfile" + source.Kind).GetLocalizedResource();
    public string Count => (source.IsApproximate ? "≈ " : string.Empty) + source.Count.ToString("N0");

    /// <summary> Обновляет подпись типа медиа при переключении языка </summary>
    public void RefreshLanguage() => OnPropertyChanged(nameof(Caption));
}
