namespace TgStorage.Domain.ChatUsers;

/// <summary> EF chat user entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(ChatId))]
[Index(nameof(UserId))]
[Index(nameof(ChatId), nameof(UserId), IsUnique = true)]
[Index(nameof(Role))]
[Index(nameof(JoinedAt))]
[Index(nameof(IsMuted))]
[Index(nameof(IsDeleted))]
public sealed class TgEfChatUserEntity : ITgEfChatUserEntity
{
    #region Fields, properties, constructor

    [DefaultValue("00000000-0000-0000-0000-000000000000")]
    [Key]
    [Required]
    [Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
    [SQLite.Collation("NOCASE")]
    public Guid Uid { get; set; }

    [DefaultValue("0001-01-01 00:00:00")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnDtChanged, TypeName = "DATETIME")]
    public DateTime DtChanged { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnChatId, TypeName = "LONG(20)")]
    public long ChatId { get; set; }

    [ForeignKey(nameof(ChatId))]
    public TgEfSourceEntity? Chat { get; set; }

    [DefaultValue(0)]
    [Column(TgEfConstants.ColumnUserId, TypeName = "LONG(20)")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public TgEfUserEntity? User { get; set; }

    [DefaultValue(TgEnumChatUserRole.Member)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnRole, TypeName = "INT")]
    public TgEnumChatUserRole Role { get; set; } = TgEnumChatUserRole.Member;

    [DefaultValue("0001-01-01 00:00:00")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnJoinedAt, TypeName = "DATETIME")]
    public DateTime JoinedAt { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsMuted, TypeName = "BIT")]
    public bool IsMuted { get; set; }

    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnMutedUntil, TypeName = "DATETIME")]
    public DateTime? MutedUntil { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsDeleted, TypeName = "BIT")]
    public bool IsDeleted { get; set; }

    public TgEfChatUserEntity() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    /// <inheritdoc />
    public void Default()
    {
        Uid = this.GetDefaultPropertyGuid(nameof(Uid));
        DtChanged = this.GetDefaultPropertyDateTime(nameof(DtChanged));
        ChatId = this.GetDefaultPropertyLong(nameof(ChatId));
        Chat = null;
        UserId = this.GetDefaultPropertyLong(nameof(UserId));
        User = null;
        Role = this.GetDefaultPropertyGeneric<TgEnumChatUserRole>(nameof(Role));
        JoinedAt = this.GetDefaultPropertyDateTime(nameof(JoinedAt));
        IsMuted = this.GetDefaultPropertyBool(nameof(IsMuted));
        MutedUntil = null;
        IsDeleted = this.GetDefaultPropertyBool(nameof(IsDeleted));
    }

    #endregion
}
