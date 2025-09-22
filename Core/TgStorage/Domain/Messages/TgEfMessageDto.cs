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
    public partial long UserId { get; set; }
    [ObservableProperty]
    public partial string UserContact { get; set; }
    [ObservableProperty]
    public partial bool IsDeleted { get; set; }

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
                return string.Empty;
            return Message[(idx + 1)..].Trim();
        }
    }

    public string ImageFullPath
    {
        get
        {
            try
            {
                if (string.IsNullOrEmpty(Directory)) return string.Empty;
                if (string.IsNullOrEmpty(FileName)) return string.Empty;

                var fullPath = Path.Combine(Directory, FileName);
                if (File.Exists(fullPath) && !fullPath.EndsWith(TgFileUtils.ExtensionThumbnail) &&
                    (fullPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                    return fullPath;
                return string.Empty;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
            }
#else
        catch (Exception)
        {
            //
        }
#endif
            return string.Empty;
        }
    }

    public string ThumbFullPath
    {
        get
        {
            try
            {
                if (string.IsNullOrEmpty(Directory)) return string.Empty;
                if (string.IsNullOrEmpty(FileName)) return string.Empty;

                var fullPath = Path.Combine(Directory, FileName);
                if (File.Exists(fullPath) && fullPath.EndsWith(TgFileUtils.ExtensionThumbnail, StringComparison.OrdinalIgnoreCase))
                    return fullPath;
                return string.Empty;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
            }
#else
        catch (Exception)
        {
            //
        }
#endif
            return string.Empty;
        }
    }

    public bool IsImageExists => !string.IsNullOrEmpty(ImageFullPath);

    public bool IsThumbExists => !string.IsNullOrEmpty(ThumbFullPath);

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
