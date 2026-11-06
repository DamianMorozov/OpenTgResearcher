namespace TgStorage.Domain.Messages;

/// <summary> EF message DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageDto : TgDtoBase, ITgEfMessageDto
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtCreated { get; set; }
    public string DtChangedString => $"{DtCreated:yyyy-MM-dd HH:mm:ss}";
    [ObservableProperty]
	public partial long SourceId { get; set; }
	[ObservableProperty]
	public partial int Id { get; set; }
	[ObservableProperty]
	public partial TgEnumMessageType Type { get; set; }
	[ObservableProperty]
	public partial long Size { get; set; }
	[ObservableProperty]
	public partial string Message { get; set; }
	[ObservableProperty]
	public partial TgEnumDirection Direction { get; set; }
    [ObservableProperty]
    public partial string Directory { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SenderDisplay))]
    public partial long UserId { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SenderDisplay))]
    public partial string UserContact { get; set; }
    [ObservableProperty]
    public partial bool IsDeleted { get; set; }

    /// <summary> Показывает название чата для сообщения без ID автора, иначе идентификатор отправителя </summary>
    public string SenderDisplay => UserId == 0 ? (string.IsNullOrWhiteSpace(UserContact) ? SourceId.ToString() : UserContact) : UserId.ToString();

    public string MessageText
    {
        get
        {
            if (string.IsNullOrEmpty(Message))
                return string.Empty;
            var idx = Message.LastIndexOf('|');
            if (idx < 0)
                return Message.Trim();
            return Message[..idx].Trim();
        }
    }

    public string FileName
    {
        get
        {
            if (string.IsNullOrEmpty(Message))
                return string.Empty;
            var idx = Message.LastIndexOf('|');
            if (idx < 0)
                return Type == TgEnumMessageType.Document ? Message.Trim() : string.Empty;
            return Message[(idx + 1)..].Trim();
        }
    }

    public string ImageFullPath
    {
        get
        {
            try
            {
                var fullPath = FindMediaFile(isThumbnail: false);
                if (!fullPath.EndsWith(TgFileUtils.ExtensionThumbnail, StringComparison.OrdinalIgnoreCase) &&
                    (fullPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                    return fullPath;
                return string.Empty;
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
            }
            return string.Empty;
        }
    }

    public string ThumbFullPath
    {
        get
        {
            try
            {
                return FindMediaFile(isThumbnail: true);
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
            }
            return string.Empty;
        }
    }

    public bool IsImageExists => !string.IsNullOrEmpty(ImageFullPath);

    public bool IsThumbExists => !string.IsNullOrEmpty(ThumbFullPath);

    // Находим непустой файл или его обложку, включая исторические имена без ID в сообщении
    private string FindMediaFile(bool isThumbnail)
    {
        if (string.IsNullOrWhiteSpace(Directory) || string.IsNullOrWhiteSpace(FileName)) return string.Empty;
        var fileName = FileName;
        var fullPath = Path.IsPathFullyQualified(fileName) ? fileName : Path.Combine(Directory, fileName.TrimStart('\\', '/'));
        if (isThumbnail && !fullPath.EndsWith(TgFileUtils.ExtensionThumbnail, StringComparison.OrdinalIgnoreCase))
            fullPath = Path.Combine(Path.GetDirectoryName(fullPath)!, Path.GetFileNameWithoutExtension(fullPath) + TgFileUtils.ExtensionThumbnail);
        if (TgFileUtils.CalculateFileSize(fullPath) > 0) return fullPath;
        var folder = Path.GetDirectoryName(fullPath)!;
        if (Id <= 0 || !System.IO.Directory.Exists(folder)) return string.Empty;
        var localName = Path.GetFileName(fullPath);
        var separator = localName.IndexOf(' ');
        if (separator > 0 && long.TryParse(localName[..separator], out var prefixId) && prefixId == Id)
        {
            localName = localName[(separator + 1)..];
            var withoutId = Path.Combine(folder, localName);
            if (TgFileUtils.CalculateFileSize(withoutId) > 0) return withoutId;
        }
        var suffix = " " + localName;
        foreach (var candidate in System.IO.Directory.EnumerateFiles(folder, "*" + suffix))
        {
            var name = Path.GetFileName(candidate);
            if (long.TryParse(name[..^suffix.Length], out var messageId) && messageId == Id && TgFileUtils.CalculateFileSize(candidate) > 0)
                return candidate;
        }
        return string.Empty;
    }

    public string Link => TgStringUtils.FormatChatLink(string.Empty, SourceId, (int)Id).Item2;

    public TgEfMessageDto() : base()
	{
		DtCreated = DateTime.MinValue;
		SourceId = 0;
		Id = 0;
		Type = TgEnumMessageType.Message;
		Size = 0;
		Message = string.Empty;
        UserId = 0;
        IsDeleted = false;
        Direction = TgEnumDirection.Default;
        Directory = string.Empty;
        UserContact = string.Empty;
    }

	#endregion
}
