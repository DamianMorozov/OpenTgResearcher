﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Models;

/// <summary>
/// App xml.
/// </summary>
[Serializable]
[XmlRoot("App", Namespace = "", IsNullable = true)]
[DebuggerDisplay("{ToDebugString()}")]
public class TgAppXmlModel : ObservableObject, ITgCommon
{
	#region Public and private fields, properties, constructor

	[DefaultValue("")]
	[XmlIgnore]
	public string Version { get; set; }
	[DefaultValue("")]
	[XmlElement]
	public string FileSession { get; set; }
	[XmlIgnore]
	public bool IsExistsFileSession => File.Exists(FileSession);
	[DefaultValue("")]
	[XmlElement]
	public string FileStorage { get; set; }
	[XmlIgnore]
	public bool IsExistsFileStorage => File.Exists(FileStorage) && new FileInfo(FileStorage).Length > 0;
	[DefaultValue("")]
	[XmlElement]
	public string TestStorage { get; set; }
	[XmlIgnore]
	public bool IsExistsTestStorage => File.Exists(TestStorage);
	[XmlIgnore] 
	public bool IsReady => IsExistsFileSession && IsExistsFileStorage;
	[DefaultValue(false)]
	[XmlElement]
	public bool IsUseProxy { get; set; }

	public TgAppXmlModel()
	{
		if (string.IsNullOrEmpty(Version))
			Version = this.GetDefaultPropertyString(nameof(Version));
		FileSession = this.GetDefaultPropertyString(nameof(FileSession));
		FileStorage = this.GetDefaultPropertyString(nameof(FileStorage));
		TestStorage = this.GetDefaultPropertyString(nameof(TestStorage));
		IsUseProxy = this.GetDefaultPropertyBool(nameof(IsUseProxy));
	}

	#endregion

	#region Public and private methods

    public string ToDebugString() => $"{TgCommonUtils.GetIsReady(IsReady)} | {TgCommonUtils.GetIsUseProxy(IsUseProxy)}";

	/// <summary>
	/// Set version from assembly.
	/// </summary>
	/// <param name="assembly"></param>
	public void SetVersion(Assembly assembly)
	{
		Version = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? string.Empty;
		ushort count = 0, pos = 0;
		foreach (char c in Version)
		{
			if (Equals(c, '.'))
				count++;
			if (count is 3)
				break;
			pos++;
		}
		if (count is 3)
			Version = Version.Substring(0, pos);
		Version = $"v{Version}";
	}

	/// <summary>
	///  Set path for file session.
	/// </summary>
	/// <param name="path"></param>
	public void SetFileSessionPath(string path)
	{
		FileSession = !File.Exists(path) && Directory.Exists(path)
			? Path.Combine(path, TgFileUtils.Session)
			: path;
		if (!IsExistsFileSession)
		{
			FileSession = Path.Combine(Directory.GetCurrentDirectory(), TgFileUtils.Session);
		}
	}

	/// <summary>
	/// Set path for file storage.
	/// </summary>
	/// <param name="path"></param>
	public void SetFileStoragePath(string path)
	{
		FileStorage = !File.Exists(path) && Directory.Exists(path)
			? Path.Combine(path, TgFileUtils.Storage)
			: path;
		if (!IsExistsFileStorage)
		{
			FileStorage = Path.Combine(Directory.GetCurrentDirectory(), TgFileUtils.Storage);
		}
	}

	/// <summary>
	/// Set path for test storage.
	/// </summary>
	/// <param name="path"></param>
	public void SetTestStoragePath(string path)
	{
		TestStorage = !File.Exists(path) && Directory.Exists(path)
			? Path.Combine(path, TgFileUtils.Storage)
			: path;
		if (!IsExistsTestStorage)
		{
			TestStorage = Path.Combine(Directory.GetCurrentDirectory(), TgFileUtils.Storage);
		}
	}

	public override string ToString() =>
		$"{nameof(TgAppXmlModel)} | {FileSession} | {FileStorage} | {TestStorage} | {IsUseProxy}";

	#endregion
}