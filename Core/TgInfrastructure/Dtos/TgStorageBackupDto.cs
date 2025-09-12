namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageBackupDto : ObservableRecipient
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial string FileName { get; private set; } = string.Empty;
	[ObservableProperty]
	public partial long FileSize { get; set; }
	[ObservableProperty]
	public partial string FileSizeAsString { get; set; }

    public TgStorageBackupDto(string fileName, long fileSize)
    {
        FileName = fileName;
        FileSize = fileSize;
        FileSizeAsString = TgFileUtils.GetFileSizeAsString(fileSize);
    }

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    #endregion
}
