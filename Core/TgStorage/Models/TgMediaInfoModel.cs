// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Models;

public sealed class TgMediaInfoModel(string remote, long size, DateTime dtCreate)
{
	public string RemoteName { get; set; } = remote;
	public long RemoteSize { get; set; } = size;
	public DateTime DtCreate { get; set; } = dtCreate;
	public string Number { get; set; } = string.Empty;
	public bool IsJoinFileNameWithMessageId { get; set; }
	public string LocalNameOnly { get; set; } = remote;
	public string LocalNameWithNumber => IsJoinFileNameWithMessageId ? $"{Number} {LocalNameOnly}" : LocalNameOnly;
	public string LocalPathOnly { get; set; } = string.Empty;
	public string LocalPathWithNumber => Path.Combine(LocalPathOnly, LocalNameWithNumber);
	private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

	public TgMediaInfoModel() : this(string.Empty, 0, default) { }

	public void Normalize(bool isJoinFileNameWithMessageId)
	{
		IsJoinFileNameWithMessageId = isJoinFileNameWithMessageId;
		LocalNameOnly = LocalNameOnly.Trim();
		// Replace characters in the file name depending on the OS
		LocalNameOnly = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LocalNameOnly.Replace("/", "\\") : LocalNameOnly.Replace("\\", "/");
		// Replace invalid characters for file names 
		LocalNameOnly = InvalidChars.Aggregate(LocalNameOnly, (current, c) => current.Replace(c.ToString(), "_"));
	}
}