// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageDto : TgSensitiveDto, ITgDto<TgEfMessageEntity, TgEfMessageDto>
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
                if (File.Exists(fullPath))
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

    public string Link => TgStringUtils.FormatChatLink(string.Empty, SourceId, (int)Id).Item2;

    public TgEfMessageDto() : base()
	{
		DtCreated = DateTime.MinValue;
		SourceId = 0;
		Id = 0;
		Type = TgEnumMessageType.Message;
		Size = 0;
		Message = string.Empty;
        Direction = TgEnumDirection.Default;
        Directory = string.Empty;
        UserId = 0;
        IsDeleted = false;
    }

	#endregion

	#region Methods

	public TgEfMessageDto Copy(TgEfMessageDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		DtCreated = dto.DtCreated;
		SourceId = dto.SourceId;
		Id = dto.Id;
        Type = dto.Type;
		Size = dto.Size;
		Message = dto.Message;
		Direction = dto.Direction;
        Directory = dto.Directory;
        UserId = dto.UserId;
        IsDeleted = dto.IsDeleted;
        return this;
	}

	public TgEfMessageDto Copy(TgEfMessageEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtCreated = item.DtCreated;
		SourceId = item.SourceId;
		Id = item.Id;
        Type = item.Type;
		Size = item.Size;
		Message = item.Message;
		Direction = TgEnumDirection.Default;
        Directory = string.Empty;
        UserId = item.UserId;
        IsDeleted = item.IsDeleted;
        return this;
	}

	public TgEfMessageDto GetNewDto(TgEfMessageEntity item) => new TgEfMessageDto().Copy(item, isUidCopy: true);

	public TgEfMessageEntity GetNewEntity(TgEfMessageDto dto) => new()
	{
		Uid = dto.Uid,
		DtCreated = dto.DtCreated,
		SourceId = dto.SourceId,
		Id = dto.Id,
		Type = dto.Type,
		Size = dto.Size,
		Message = dto.Message,
        UserId = dto.UserId,
        IsDeleted = dto.IsDeleted,
    };

	public TgEfMessageEntity GetNewEntity() => new()
	{
		Uid = Uid,
		DtCreated = DtCreated,
		SourceId = SourceId,
		Id = Id,
		Type = Type,
		Size = Size,
		Message = Message,
        UserId = UserId,
        IsDeleted = IsDeleted,
    };

    #endregion
}
