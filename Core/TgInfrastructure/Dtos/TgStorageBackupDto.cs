// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageBackupDto : ObservableRecipient
{
	#region Public and private fields, properties, constructor

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

    #region Public and private methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    #endregion
}
