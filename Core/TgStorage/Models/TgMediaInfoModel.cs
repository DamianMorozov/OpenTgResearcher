namespace TgStorage.Models;

public sealed class TgMediaInfoModel(string remote, long size, DateTime dtCreate)
{
    #region Fields, properties, constructor

    public string RemoteName { get; set; } = remote;
    public long RemoteSize { get; set; } = size;
    public DateTime DtCreate { get; set; } = dtCreate;
    public string Number { get; set; } = string.Empty;
    public bool IsJoinFileNameWithMessageId { get; set; }
    public string LocalNameOnly { get; set; } = remote;
    public string LocalNameWithNumber => IsJoinFileNameWithMessageId ? $"{Number} {LocalNameOnly}" : LocalNameOnly;
    public string LocalPathOnly { get; set; } = string.Empty;
    public string LocalPathWithNumber => Path.Combine(LocalPathOnly, LocalNameWithNumber);
    public string ThumbnailPathWithNumber => Path.Combine(LocalPathOnly, Path.GetFileNameWithoutExtension(LocalNameWithNumber) + TgFileUtils.ExtensionThumbnail);
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public TgMediaInfoModel() : this(string.Empty, 0, default) { }

    #endregion

    #region Methods

    public void Normalize(bool isJoinFileNameWithMessageId)
    {
        IsJoinFileNameWithMessageId = isJoinFileNameWithMessageId;

        // Trim whitespace and replace path separators according to OS
        LocalNameOnly = LocalNameOnly.Trim();
        LocalNameOnly = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? LocalNameOnly.Replace("/", "\\")
            : LocalNameOnly.Replace("\\", "/");

        // Substitute invalid filename chars with underscore
        foreach (var c in InvalidChars)
            LocalNameOnly = LocalNameOnly.Replace(c.ToString(), "_");

        // Guarantee single extension at the end of name
        LocalNameOnly = TgStringUtils.EnsureSingleExtension(LocalNameOnly);
    }

    #endregion
}
