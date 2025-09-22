namespace TgStorage.Domain.Documents;

/// <summary> EF contact DTO </summary>
public sealed partial class TgEfDocumentDto : TgDtoBase, ITgEfDocumentDto
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial long SourceId { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial int MessageId { get; set; }
	[ObservableProperty]
	public partial string FileName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial long FileSize { get; set; }
	[ObservableProperty]
	public partial long AccessHash { get; set; }

	public TgEfDocumentDto() : base()
	{
		SourceId = 0;
		Id = 0;
		MessageId = 0;
		FileName = string.Empty;
		FileSize = 0;
		AccessHash = 0;
	}

    #endregion

    #region Private methods

    /// <inheritdoc />
    public override string ToString() => $"{SourceId} | {Id} | {AccessHash}";

	#endregion
}
