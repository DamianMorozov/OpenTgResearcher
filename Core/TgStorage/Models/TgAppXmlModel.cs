// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Models;

/// <summary> App xml </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Serializable]
[XmlRoot("App", Namespace = "", IsNullable = true)]
public sealed class TgAppXmlModel : ObservableObject, ITgCommon
{
	#region Public and private fields, properties, constructor

	[DefaultValue("")]
	[XmlElement("FileSession")]
	public string XmlFileSession { get; set; } = null!;
	[DefaultValue("")]
	[XmlElement("EfStorage")]
	public string XmlEfStorage { get; set; } = null!;

	[XmlIgnore]
	public bool IsExistsFileSession => File.Exists(XmlFileSession);
	[XmlIgnore]
	public bool IsExistsEfStorage => File.Exists(XmlEfStorage) && new FileInfo(XmlEfStorage).Length > 0;

	public TgAppXmlModel()
	{
		Default();
	}

	#endregion

	#region Public and private methods

    public string ToDebugString() => 
	    $"{nameof(XmlFileSession)}: {XmlFileSession} | {nameof(XmlEfStorage)}: {XmlEfStorage}";

    public void Default()
    {
	    XmlFileSession = TgFileUtils.FileTgSession;
	    XmlEfStorage = TgEfUtils.FileEfStorage;
	}

	/// <summary> Set path for file session </summary>
	public void SetFileSessionPath(string path)
	{
		XmlFileSession = !File.Exists(path) && Directory.Exists(path)
			? Path.Combine(path, TgFileUtils.FileTgSession)
			: path;
		if (!IsExistsFileSession)
		{
			XmlFileSession = Path.Combine(Directory.GetCurrentDirectory(), TgFileUtils.FileTgSession);
		}
	}

	/// <summary> Set path for file storage </summary>
	public void SetEfStoragePath(string path)
	{
		XmlEfStorage = !File.Exists(path) && Directory.Exists(path)
			? Path.Combine(path, TgEfUtils.FileEfStorage)
			: path;
		if (!IsExistsEfStorage)
		{
			XmlEfStorage = Path.Combine(Directory.GetCurrentDirectory(), TgEfUtils.FileEfStorage);
		}
	}

	#endregion
}