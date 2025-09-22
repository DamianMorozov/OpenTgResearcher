namespace TgStorage.Helpers;

/// <summary> Utility methods for creating copies of domain and DTO objects </summary>
public static class TgEfDomainUtils
{
    #region Methods - Generics

    /// <summary> Create an entity from DTO </summary>
    public static ITgEfEntity CreateNewEntity(ITgDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto switch
        {
            ITgEfAppDto => CreateNewEntity((TgEfAppDto)dto, isUidCopy),
            ITgEfChatUserDto => CreateNewEntity((TgEfChatUserDto)dto, isUidCopy),
            ITgEfDocumentDto => CreateNewEntity((TgEfDocumentDto)dto, isUidCopy),
            ITgEfFilterDto => CreateNewEntity((TgEfFilterDto)dto, isUidCopy),
            ITgEfLicenseDto => CreateNewEntity((TgEfLicenseDto)dto, isUidCopy),
            ITgEfMessageDto => CreateNewEntity((TgEfMessageDto)dto, isUidCopy),
            ITgEfMessageRelationDto => CreateNewEntity((TgEfMessageRelationDto)dto, isUidCopy),
            ITgEfProxyDto => CreateNewEntity((TgEfProxyDto)dto, isUidCopy),
            ITgEfSourceDto => CreateNewEntity((TgEfSourceDto)dto, isUidCopy),
            ITgEfStoryDto => CreateNewEntity((TgEfStoryDto)dto, isUidCopy),
            ITgEfUserDto => CreateNewEntity((TgEfUserDto)dto, isUidCopy),
            ITgEfVersionDto => CreateNewEntity((TgEfVersionDto)dto, isUidCopy),
            _ => throw new ArgumentException("Wrong type!", nameof(dto)),
        };
    }

    /// <summary> Create an entity from entity </summary>
    public static ITgEfEntity CreateNewEntity(ITgEfEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity switch
        {
            ITgEfAppEntity => CreateNewEntity((TgEfAppEntity)entity, isUidCopy),
            ITgEfChatUserEntity => CreateNewEntity((TgEfChatUserEntity)entity, isUidCopy),
            ITgEfDocumentEntity => CreateNewEntity((TgEfDocumentEntity)entity, isUidCopy),
            ITgEfFilterEntity => CreateNewEntity((TgEfFilterEntity)entity, isUidCopy),
            ITgEfLicenseEntity => CreateNewEntity((TgEfLicenseEntity)entity, isUidCopy),
            ITgEfMessageEntity => CreateNewEntity((TgEfMessageEntity)entity, isUidCopy),
            ITgEfMessageRelationEntity => CreateNewEntity((TgEfMessageRelationEntity)entity, isUidCopy),
            ITgEfProxyEntity => CreateNewEntity((TgEfProxyEntity)entity, isUidCopy),
            ITgEfSourceEntity => CreateNewEntity((TgEfSourceEntity)entity, isUidCopy),
            ITgEfStoryEntity => CreateNewEntity((TgEfStoryEntity)entity, isUidCopy),
            ITgEfUserEntity => CreateNewEntity((TgEfUserEntity)entity, isUidCopy),
            ITgEfVersionEntity => CreateNewEntity((TgEfVersionEntity)entity, isUidCopy),
            _ => throw new ArgumentException("Wrong type!", nameof(entity)),
        };
    }

    /// <summary> Create a DTO from DTO </summary>
    public static ITgDto CreateNewDto(ITgDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto switch
        {
            ITgEfAppDto => CreateNewDto((TgEfAppDto)dto, isUidCopy),
            ITgEfChatUserDto => CreateNewDto((TgEfChatUserDto)dto, isUidCopy),
            ITgEfDocumentDto => CreateNewDto((TgEfDocumentDto)dto, isUidCopy),
            ITgEfFilterDto => CreateNewDto((TgEfFilterDto)dto, isUidCopy),
            ITgEfLicenseDto => CreateNewDto((TgEfLicenseDto)dto, isUidCopy),
            ITgEfMessageDto => CreateNewDto((TgEfMessageDto)dto, isUidCopy),
            ITgEfMessageRelationDto => CreateNewDto((TgEfMessageRelationDto)dto, isUidCopy),
            ITgEfProxyDto => CreateNewDto((TgEfProxyDto)dto, isUidCopy),
            ITgEfSourceDto => CreateNewDto((TgEfSourceDto)dto, isUidCopy),
            ITgEfStoryDto => CreateNewDto((TgEfStoryDto)dto, isUidCopy),
            ITgEfUserDto => CreateNewDto((TgEfUserDto)dto, isUidCopy),
            ITgEfVersionDto => CreateNewDto((TgEfVersionDto)dto, isUidCopy),
            _ => throw new ArgumentException("Wrong type!", nameof(dto)),
        };
    }

    /// <summary> Create a DTO from entity </summary>
    public static ITgDto CreateNewDto(ITgEfEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity switch
        {
            ITgEfAppEntity => CreateNewDto((TgEfAppEntity)entity, isUidCopy),
            ITgEfChatUserEntity => CreateNewDto((TgEfChatUserEntity)entity, isUidCopy),
            ITgEfDocumentEntity => CreateNewDto((TgEfDocumentEntity)entity, isUidCopy),
            ITgEfFilterEntity => CreateNewDto((TgEfFilterEntity)entity, isUidCopy),
            ITgEfLicenseEntity => CreateNewDto((TgEfLicenseEntity)entity, isUidCopy),
            ITgEfMessageEntity => CreateNewDto((TgEfMessageEntity)entity, isUidCopy),
            ITgEfMessageRelationEntity => CreateNewDto((TgEfMessageRelationEntity)entity, isUidCopy),
            ITgEfProxyEntity => CreateNewDto((TgEfProxyEntity)entity, isUidCopy),
            ITgEfSourceEntity => CreateNewDto((TgEfSourceEntity)entity, isUidCopy),
            ITgEfStoryEntity => CreateNewDto((TgEfStoryEntity)entity, isUidCopy),
            ITgEfUserEntity => CreateNewDto((TgEfUserEntity)entity, isUidCopy),
            ITgEfVersionEntity => CreateNewDto((TgEfVersionEntity)entity, isUidCopy),
            _ => throw new ArgumentException("Wrong type!", nameof(entity)),
        };
    }

    #endregion

    #region Methods - Apps

    /// <summary> Create an entity from DTO </summary>
    public static TgEfAppEntity CreateNewEntity(TgEfAppDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfAppEntity
        {
            ApiHash = dto.ApiHash,
            ApiId = dto.ApiId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ProxyUid = dto.ProxyUid,
            BotTokenKey = dto.BotTokenKey,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        if (dto.UseBot)
            target.SetUseBot(target.UseBot);
        else if (target.UseClient)
            target.SetUseClient(target.UseClient);

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfAppEntity CreateNewEntity(TgEfAppEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfAppEntity
        {
            ApiHash = entity.ApiHash,
            ApiId = entity.ApiId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PhoneNumber = entity.PhoneNumber,
            ProxyUid = entity.ProxyUid,
            BotTokenKey = entity.BotTokenKey,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        if (entity.UseBot)
            target.SetUseBot(target.UseBot);
        else if (target.UseClient)
            target.SetUseClient(target.UseClient);

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfAppDto CreateNewDto(TgEfAppDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfAppDto
        {
            ApiHash = dto.ApiHash,
            ApiId = dto.ApiId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ProxyUid = dto.ProxyUid,
            UseBot = dto.UseBot,
            BotTokenKey = dto.BotTokenKey,
            UseClient = dto.UseClient,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfAppDto CreateNewDto(TgEfAppEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfAppDto
        {
            ApiHash = entity.ApiHash,
            ApiId = entity.ApiId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PhoneNumber = entity.PhoneNumber,
            ProxyUid = entity.ProxyUid ?? Guid.Empty,
            UseBot = entity.UseBot,
            BotTokenKey = entity.BotTokenKey,
            UseClient = entity.UseClient,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Chat users

    /// <summary> Create an entity from DTO </summary>
    public static TgEfChatUserEntity CreateNewEntity(TgEfChatUserDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = new TgEfChatUserEntity
        {
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            ChatId = dto.ChatId,
            UserId = dto.UserId,
            Role = dto.Role,
            JoinedAt = dto.JoinedAt,
            IsMuted = dto.IsMuted,
            MutedUntil = dto.MutedUntil,
            IsDeleted = dto.IsDeleted
        };

        if (isUidCopy)
            entity.Uid = dto.Uid;

        return entity;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfChatUserEntity CreateNewEntity(TgEfChatUserEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfChatUserEntity
        {
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            ChatId = entity.ChatId,
            UserId = entity.UserId,
            Role = entity.Role,
            JoinedAt = entity.JoinedAt,
            IsMuted = entity.IsMuted,
            MutedUntil = entity.MutedUntil,
            IsDeleted = entity.IsDeleted
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        if (entity.Chat is not null)
            target.Chat = CreateNewEntity(entity.Chat, isUidCopy: isUidCopy);
        if (entity.User is not null)
            target.User = CreateNewEntity(entity.User, isUidCopy: isUidCopy);

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfChatUserDto CreateNewDto(TgEfChatUserDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfChatUserDto
        {
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            ChatId = dto.ChatId,
            UserId = dto.UserId,
            Role = dto.Role,
            JoinedAt = dto.JoinedAt,
            IsMuted = dto.IsMuted,
            MutedUntil = dto.MutedUntil,
            IsDeleted = dto.IsDeleted
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfChatUserDto CreateNewDto(TgEfChatUserEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new TgEfChatUserDto
        {
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            ChatId = entity.ChatId,
            UserId = entity.UserId,
            Role = entity.Role,
            JoinedAt = entity.JoinedAt,
            IsMuted = entity.IsMuted,
            MutedUntil = entity.MutedUntil,
            IsDeleted = entity.IsDeleted
        };

        if (isUidCopy)
            dto.Uid = entity.Uid;

        return dto;
    }

    #endregion

    #region Methods - Filters

    /// <summary> Create an entity from DTO </summary>
    public static TgEfFilterEntity CreateNewEntity(TgEfFilterDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfFilterEntity()
        {
            IsEnabled = dto.IsEnabled,
            FilterType = dto.FilterType,
            Name = dto.Name,
            Mask = dto.Mask,
            Size = dto.Size,
            SizeType = dto.SizeType,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfFilterEntity CreateNewEntity(TgEfFilterEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfFilterEntity
        {
            IsEnabled = entity.IsEnabled,
            FilterType = entity.FilterType,
            Name = entity.Name,
            Mask = string.IsNullOrEmpty(entity.Mask) &&
                (Equals(entity.FilterType, TgEnumFilterType.MinSize) ||
                Equals(entity.FilterType, TgEnumFilterType.MaxSize)) ? "*" : entity.Mask,
            Size = entity.Size,
            SizeType = entity.SizeType,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfFilterDto CreateNewDto(TgEfFilterDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfFilterDto
        {
            IsEnabled = dto.IsEnabled,
            FilterType = dto.FilterType,
            Name = dto.Name,
            Mask = dto.Mask,
            Size = dto.Size,
            SizeType = dto.SizeType,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfFilterDto CreateNewDto(TgEfFilterEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfFilterDto
        {
            IsEnabled = entity.IsEnabled,
            FilterType = entity.FilterType,
            Name = entity.Name,
            Mask = entity.Mask,
            Size = entity.Size,
            SizeType = entity.SizeType,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Documents

    /// <summary> Create an entity from DTO </summary>
    public static TgEfDocumentEntity CreateNewEntity(TgEfDocumentDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfDocumentEntity
        {
            SourceId = dto.SourceId,
            Id = dto.Id,
            MessageId = dto.MessageId,
            FileName = dto.FileName,
            FileSize = dto.FileSize,
            AccessHash = dto.AccessHash,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfDocumentEntity CreateNewEntity(TgEfDocumentEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfDocumentEntity
        {
            SourceId = entity.SourceId,
            Id = entity.Id,
            MessageId = entity.MessageId,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            AccessHash = entity.AccessHash,
        };

        if (entity.Source is not null)
            target.Source = CreateNewEntity(entity.Source, isUidCopy: isUidCopy);

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfDocumentDto CreateNewDto(TgEfDocumentDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfDocumentDto
        {
            SourceId = dto.SourceId,
            Id = dto.Id,
            MessageId = dto.MessageId,
            FileName = dto.FileName,
            FileSize = dto.FileSize,
            AccessHash = dto.AccessHash,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfDocumentDto CreateNewDto(TgEfDocumentEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfDocumentDto
        {
            SourceId = entity.SourceId,
            Id = entity.Id,
            MessageId = entity.MessageId,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            AccessHash = entity.AccessHash,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Licenses

    /// <summary> Create an entity from DTO </summary>
    public static TgEfLicenseEntity CreateNewEntity(TgEfLicenseDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfLicenseEntity
        {
            LicenseKey = dto.LicenseKey,
            UserId = dto.UserId,
            LicenseType = dto.LicenseType,
            ValidTo = DateTime.Parse($"{dto.ValidTo:yyyy-MM-d}"),
            IsConfirmed = dto.IsConfirmed,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfLicenseEntity CreateNewEntity(TgEfLicenseEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfLicenseEntity
        {
            LicenseKey = entity.LicenseKey,
            UserId = entity.UserId,
            LicenseType = entity.LicenseType,
            ValidTo = entity.ValidTo,
            IsConfirmed = entity.IsConfirmed,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfLicenseDto CreateNewDto(TgEfLicenseDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfLicenseDto
        {
            LicenseKey = dto.LicenseKey,
            UserId = dto.UserId,
            LicenseType = dto.LicenseType,
            ValidTo = dto.ValidTo,
            IsConfirmed = dto.IsConfirmed,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfLicenseDto CreateNewDto(TgEfLicenseEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfLicenseDto
        {
            LicenseKey = entity.LicenseKey,
            UserId = entity.UserId,
            LicenseType = entity.LicenseType,
            ValidTo = DateOnly.FromDateTime(entity.ValidTo),
            IsConfirmed = entity.IsConfirmed,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Messages

    /// <summary> Create an entity from DTO </summary>
    public static TgEfMessageEntity CreateNewEntity(TgEfMessageDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfMessageEntity
        {
            DtCreated = dto.DtCreated > DateTime.MinValue ? dto.DtCreated : DateTime.UtcNow,
            SourceId = dto.SourceId,
            Id = dto.Id,
            Type = dto.Type,
            Size = dto.Size,
            Message = dto.Message,
            UserId = dto.UserId,
            IsDeleted = dto.IsDeleted,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfMessageEntity CreateNewEntity(TgEfMessageEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfMessageEntity
        {
            DtCreated = entity.DtCreated > DateTime.MinValue ? entity.DtCreated : DateTime.UtcNow,
            SourceId = entity.SourceId,
            Id = entity.Id,
            Type = entity.Type,
            Size = entity.Size,
            Message = entity.Message,
            UserId = entity.UserId,
            IsDeleted = entity.IsDeleted,
        };

        if (entity.Source is not null)
            target.Source = CreateNewEntity(entity.Source, isUidCopy: isUidCopy);

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfMessageDto CreateNewDto(TgEfMessageDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfMessageDto
        {
            DtCreated = dto.DtCreated > DateTime.MinValue ? dto.DtCreated : DateTime.UtcNow,
            SourceId = dto.SourceId,
            Id = dto.Id,
            Type = dto.Type,
            Size = dto.Size,
            Message = dto.Message,
            UserId = dto.UserId,
            IsDeleted = dto.IsDeleted,
            Direction = dto.Direction,
            Directory = dto.Directory,
            UserContact = dto.UserContact,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfMessageDto CreateNewDto(TgEfMessageEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfMessageDto
        {
            DtCreated = entity.DtCreated > DateTime.MinValue ? entity.DtCreated : DateTime.UtcNow,
            SourceId = entity.SourceId,
            Id = entity.Id,
            Type = entity.Type,
            Size = entity.Size,
            Message = entity.Message,
            UserId = entity.UserId,
            IsDeleted = entity.IsDeleted,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Messages relations

    /// <summary> Create an entity from DTO </summary>
    public static TgEfMessageRelationEntity CreateNewEntity(TgEfMessageRelationDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfMessageRelationEntity
        {
            ParentSourceId = dto.ParentSourceId,
            ParentMessageId = dto.ParentMessageId,
            ChildSourceId = dto.ChildSourceId,
            ChildMessageId = dto.ChildMessageId,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfMessageRelationEntity CreateNewEntity(TgEfMessageRelationEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfMessageRelationEntity
        {
            ParentSourceId = entity.ParentSourceId,
            ParentMessageId = entity.ParentMessageId,
            ChildSourceId = entity.ChildSourceId,
            ChildMessageId = entity.ChildMessageId,
        };

        if (entity.ParentSource is not null)
            target.ParentSource = CreateNewEntity(entity.ParentSource, isUidCopy: isUidCopy);
        if (entity.ParentMessage is not null)
            target.ParentMessage = CreateNewEntity(entity.ParentMessage, isUidCopy: isUidCopy);
        if (entity.ChildSource is not null)
            target.ChildSource = CreateNewEntity(entity.ChildSource, isUidCopy: isUidCopy);
        if (entity.ChildMessage is not null)
            target.ChildMessage = CreateNewEntity(entity.ChildMessage, isUidCopy: isUidCopy);

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfMessageRelationDto CreateNewDto(TgEfMessageRelationDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfMessageRelationDto
        {
            ParentSourceId = dto.ParentSourceId,
            ParentMessageId = dto.ParentMessageId,
            ChildSourceId = dto.ChildSourceId,
            ChildMessageId = dto.ChildMessageId,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfMessageRelationDto CreateNewDto(TgEfMessageRelationEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfMessageRelationDto
        {
            ParentSourceId = entity.ParentSourceId,
            ParentMessageId = entity.ParentMessageId,
            ChildSourceId = entity.ChildSourceId,
            ChildMessageId = entity.ChildMessageId,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Proxies

    /// <summary> Create an entity from DTO </summary>
    public static TgEfProxyEntity CreateNewEntity(TgEfProxyDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfProxyEntity
        {
            Type = dto.Type,
            HostName = dto.HostName,
            Port = dto.Port,
            UserName = dto.UserName,
            Password = dto.Password,
            Secret = dto.Secret,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfProxyEntity CreateNewEntity(TgEfProxyEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfProxyEntity
        {
            Type = entity.Type,
            HostName = entity.HostName,
            Port = entity.Port,
            UserName = entity.UserName,
            Password = entity.Password,
            Secret = entity.Secret,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfProxyDto CreateNewDto(TgEfProxyDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfProxyDto
        {
            Type = dto.Type,
            HostName = dto.HostName,
            Port = dto.Port,
            UserName = dto.UserName,
            Password = dto.Password,
            Secret = dto.Secret,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfProxyDto CreateNewDto(TgEfProxyEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfProxyDto
        {
            Type = entity.Type,
            HostName = entity.HostName,
            Port = entity.Port,
            UserName = entity.UserName,
            Password = entity.Password,
            Secret = entity.Secret,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Sources

    /// <summary> Create an entity from DTO </summary>
    public static TgEfSourceEntity CreateNewEntity(TgEfSourceDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfSourceEntity
        {
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            Id = dto.Id,
            AccessHash = dto.AccessHash,
            IsActive = dto.IsSourceActive,
            UserName = dto.UserName,
            Title = dto.Title,
            About = dto.About,
            FirstId = dto.FirstId,
            Count = dto.Count,
            Directory = dto.Directory,
            IsAutoUpdate = dto.IsAutoUpdate,
            IsCreatingSubdirectories = dto.IsCreatingSubdirectories,
            IsUserAccess = dto.IsUserAccess,
            IsFileNamingByMessage = dto.IsFileNamingByMessage,
            //IsParsingComments = dto.IsParsingComments,
            IsRestrictSavingContent = dto.IsRestrictSavingContent,
            IsSubscribe = dto.IsSubscribe,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfSourceEntity CreateNewEntity(TgEfSourceEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfSourceEntity
        {
            Id = entity.Id,
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            AccessHash = entity.AccessHash,
            IsActive = entity.IsActive,
            FirstId = entity.FirstId,
            UserName = entity.UserName,
            Title = entity.Title,
            About = entity.About,
            Count = entity.Count,
            Directory = entity.Directory,
            IsAutoUpdate = entity.IsAutoUpdate,
            IsCreatingSubdirectories = entity.IsCreatingSubdirectories,
            IsUserAccess = entity.IsUserAccess,
            IsFileNamingByMessage = entity.IsFileNamingByMessage,
            IsRestrictSavingContent = entity.IsRestrictSavingContent,
            IsSubscribe = entity.IsSubscribe,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create an entity from DTO </summary>
    public static TgEfSourceDto CreateNewDto(TgEfSourceDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfSourceDto
        {
            Id = dto.Id,
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            AccessHash = dto.AccessHash,
            IsSourceActive = dto.IsSourceActive,
            FirstId = dto.FirstId,
            UserName = dto.UserName,
            Title = dto.Title,
            About = dto.About,
            Count = dto.Count,
            Directory = dto.Directory,
            IsAutoUpdate = dto.IsAutoUpdate,
            IsCreatingSubdirectories = dto.IsCreatingSubdirectories,
            IsUserAccess = dto.IsUserAccess,
            IsFileNamingByMessage = dto.IsFileNamingByMessage,
            IsRestrictSavingContent = dto.IsRestrictSavingContent,
            IsSubscribe = dto.IsSubscribe,
            IsDownload = dto.IsDownload,
            CurrentFileName = dto.CurrentFileName,
            IsParsingComments = dto.IsParsingComments,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfSourceDto CreateNewDto(TgEfSourceEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfSourceDto
        {
            Id = entity.Id,
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            AccessHash = entity.AccessHash,
            IsSourceActive = entity.IsActive,
            FirstId = entity.FirstId,
            UserName = entity.UserName ?? string.Empty,
            Title = entity.Title ?? string.Empty,
            About = entity.About ?? string.Empty,
            Count = entity.Count,
            Directory = entity.Directory ?? string.Empty,
            IsAutoUpdate = entity.IsAutoUpdate,
            IsCreatingSubdirectories = entity.IsCreatingSubdirectories,
            IsUserAccess = entity.IsUserAccess,
            IsFileNamingByMessage = entity.IsFileNamingByMessage,
            IsRestrictSavingContent = entity.IsRestrictSavingContent,
            IsSubscribe = entity.IsSubscribe,
            //IsDownload = entity.IsDownload,
            //CurrentFileName = entity.CurrentFileName,
            //IsParsingComments = entity.IsParsingComments,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Sources lite

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfSourceLiteDto CreateNewLiteDto(TgEfSourceLiteDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfSourceLiteDto
        {
            Id = dto.Id,
            DtChangedString = dto.DtChangedString,
            UserName = dto.UserName,
            IsSourceActive = dto.IsActive,
            Title = dto.Title,
            FirstId = dto.FirstId,
            Count = dto.Count,
            IsAutoUpdate = dto.IsAutoUpdate,
            IsCreatingSubdirectories = dto.IsCreatingSubdirectories,
            IsFileNamingByMessage = dto.IsFileNamingByMessage,
            IsUserAccess = dto.IsUserAccess,
            IsDownload = dto.IsDownload,
            ProgressPercent = dto.ProgressPercent,
            ProgressPercentString = dto.ProgressPercentString,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfSourceLiteDto CreateNewLiteDto(TgEfSourceEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfSourceLiteDto
        {
            Id = entity.Id,
            DtChangedString = $"{entity.DtChanged:yyyy-MM-dd HH:mm:ss}",
            UserName = entity.UserName ?? string.Empty,
            IsSourceActive = entity.IsActive,
            Title = entity.Title ?? string.Empty,
            FirstId = entity.FirstId,
            Count = entity.Count,
            IsAutoUpdate = entity.IsAutoUpdate,
            IsCreatingSubdirectories = entity.IsCreatingSubdirectories,
            IsFileNamingByMessage = entity.IsFileNamingByMessage,
            IsRestrictSavingContent = entity.IsRestrictSavingContent,
            IsUserAccess = entity.IsUserAccess,
            //IsDownload = entity.IsDownload,
            ProgressPercent = entity.Count == 0 ? 0 : entity.FirstId * 100 / entity.Count,
            //ProgressPercentString = entity.ProgressPercentString,
            IsSubscribe = entity.IsSubscribe,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Stories

    /// <summary> Create an entity from DTO </summary>
    public static TgEfStoryEntity CreateNewEntity(TgEfStoryDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfStoryEntity
        {
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            Id = dto.Id,
            FromId = dto.FromId,
            FromName = dto.FromName,
            Date = dto.Date,
            ExpireDate = dto.ExpireDate,
            Caption = dto.Caption,
            Type = dto.Type,
            Offset = dto.Offset,
            Message = dto.Message,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfStoryEntity CreateNewEntity(TgEfStoryEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfStoryEntity
        {
            Id = entity.Id,
            FromId = entity.FromId,
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            FromName = entity.FromName,
            Date = entity.Date,
            ExpireDate = entity.ExpireDate,
            Caption = entity.Caption,
            Type = entity.Type,
            Offset = entity.Offset,
            Length = entity.Length,
            Message = entity.Message,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfStoryDto CreateNewDto(TgEfStoryDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfStoryDto
        {
            Id = dto.Id,
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            FromId = dto.FromId,
            FromName = dto.FromName,
            Date = dto.Date,
            ExpireDate = dto.ExpireDate,
            Caption = dto.Caption,
            Type = dto.Type,
            Offset = dto.Offset,
            Length = dto.Length,
            Message = dto.Message,
            IsDownload = dto.IsDownload,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfStoryDto CreateNewDto(TgEfStoryEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfStoryDto
        {
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            Id = entity.Id,
            FromId = entity.FromId ?? 0,
            FromName = entity.FromName ?? string.Empty,
            Date = entity.Date ?? DateTime.MinValue,
            ExpireDate = entity.ExpireDate ?? DateTime.MinValue,
            Caption = entity.Caption ?? string.Empty,
            Type = entity.Type ?? string.Empty,
            Offset = entity.Offset,
            Length = entity.Length,
            Message = entity.Message ?? string.Empty,
            IsDownload = false,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods - Users

    /// <summary> Create an entity from DTO </summary>
    public static TgEfUserEntity CreateNewEntity(TgEfUserDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfUserEntity
        {
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            Id = dto.Id,
            AccessHash = dto.AccessHash,
            IsActive = dto.IsContactActive,
            IsBot = dto.IsBot,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            UserNames = dto.UserNames,
            PhoneNumber = dto.PhoneNumber,
            Status = dto.GetShortStatus(dto.Status),
            RestrictionReason = dto.RestrictionReason,
            LangCode = dto.LangCode,
            IsContact = dto.IsContact,
            IsDeleted = dto.IsDeleted,
            StoriesMaxId = dto.StoriesMaxId,
            BotInfoVersion = dto.BotInfoVersion,
            BotInlinePlaceholder = dto.BotInlinePlaceholder,
            BotActiveUsers = dto.BotActiveUsers,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfUserEntity CreateNewEntity(TgEfUserEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfUserEntity
        {
            Id = entity.Id,
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            AccessHash = entity.AccessHash,
            IsActive = entity.IsActive,
            IsBot = entity.IsBot,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            UserName = entity.UserName,
            UserNames = entity.UserNames,
            PhoneNumber = entity.PhoneNumber,
            Status = entity.Status,
            RestrictionReason = entity.RestrictionReason,
            LangCode = entity.LangCode,
            StoriesMaxId = entity.StoriesMaxId,
            BotInfoVersion = entity.BotInfoVersion,
            BotInlinePlaceholder = entity.BotInlinePlaceholder,
            BotActiveUsers = entity.BotActiveUsers,
            IsContact = entity.IsContact,
            IsDeleted = entity.IsDeleted,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfUserDto CreateNewDto(TgEfUserDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfUserDto
        {
            Id = dto.Id,
            DtChanged = dto.DtChanged > DateTime.MinValue ? dto.DtChanged : DateTime.UtcNow,
            AccessHash = dto.AccessHash,
            IsContactActive = dto.IsContactActive,
            IsBot = dto.IsBot,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            UserNames = dto.UserNames,
            PhoneNumber = dto.PhoneNumber,
            Status = dto.Status,
            RestrictionReason = dto.RestrictionReason,
            LangCode = dto.LangCode,
            IsContact = dto.IsContact,
            IsDeleted = dto.IsDeleted,
            StoriesMaxId = dto.StoriesMaxId,
            BotInfoVersion = dto.BotInfoVersion,
            BotInlinePlaceholder = dto.BotInlinePlaceholder,
            BotActiveUsers = dto.BotActiveUsers,
            IsDownload = dto.IsDownload,
            CountMessages = dto.CountMessages,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfUserDto CreateNewDto(TgEfUserEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfUserDto
        {
            Id = entity.Id,
            DtChanged = entity.DtChanged > DateTime.MinValue ? entity.DtChanged : DateTime.UtcNow,
            AccessHash = entity.AccessHash,
            IsContactActive = entity.IsActive,
            IsBot = entity.IsBot,
            FirstName = entity.FirstName ?? string.Empty,
            LastName = entity.LastName ?? string.Empty,
            UserName = entity.UserName ?? string.Empty,
            UserNames = entity.UserNames ?? string.Empty,
            PhoneNumber = entity.PhoneNumber ?? string.Empty,
            RestrictionReason = entity.RestrictionReason ?? string.Empty,
            LangCode = entity.LangCode ?? string.Empty,
            IsContact = entity.IsContact,
            IsDeleted = entity.IsDeleted,
            StoriesMaxId = entity.StoriesMaxId,
            BotInfoVersion = entity.BotInfoVersion ?? string.Empty,
            BotInlinePlaceholder = entity.BotInlinePlaceholder ?? string.Empty,
            BotActiveUsers = entity.BotActiveUsers,
            IsDownload = false,
            CountMessages = 0,
        };
        target.Status = target.GetShortStatus(entity.Status ?? string.Empty);

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfUserDto CreateNewDto(TgEfUserEntity entity, int countMessages, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return CreateNewDto(entity, isUidCopy).SetCountMessages(countMessages);
    }

    #endregion

    #region Methods - Versions

    /// <summary> Create an entity from DTO </summary>
    public static TgEfVersionEntity CreateNewEntity(TgEfVersionDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfVersionEntity
        {
            Version = dto.Version,
            Description = dto.Description,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create an entity from entity </summary>
    public static TgEfVersionEntity CreateNewEntity(TgEfVersionEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfVersionEntity
        {
            Version = entity.Version,
            Description = entity.Description,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    /// <summary> Create a DTO from DTO </summary>
    public static TgEfVersionDto CreateNewDto(TgEfVersionDto dto, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = new TgEfVersionDto
        {
            Version = dto.Version,
            Description = dto.Description,
        };

        if (isUidCopy)
            target.Uid = dto.Uid;

        return target;
    }

    /// <summary> Create a DTO from entity </summary>
    public static TgEfVersionDto CreateNewDto(TgEfVersionEntity entity, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var target = new TgEfVersionDto
        {
            Version = entity.Version,
            Description = entity.Description,
        };

        if (isUidCopy)
            target.Uid = entity.Uid;

        return target;
    }

    #endregion

    #region Methods

    /// <summary> Copies values from source entity to target entity using entity's own Copy method </summary>
    public static void CopyEntityValues(ITgEfEntity source, ITgEfEntity target, bool isUidCopy)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        dynamic dynTarget = target;
        dynamic dynSource = source;
        dynTarget.Copy(dynSource, isUidCopy: isUidCopy);
    }

    #endregion
}
