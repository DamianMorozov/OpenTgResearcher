namespace TgStorage.Helpers;

/// <summary> App helper </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgAppSettingsHelper : ITgHelper
{
	#region Design pattern "Lazy Singleton"

	private static TgAppSettingsHelper _instance = null!;
	public static TgAppSettingsHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

	#endregion

	#region Fields, properties, constructor

	[DefaultValue("")]
	public string AppVersion { get; set; }
	[DefaultValue("")]
	public string StorageVersion { get; set; }
	[DefaultValue(false)]
	public bool IsUseProxy { get; set; }
	public TgAppXmlModel AppXml { get; set; }

	public TgAppSettingsHelper()
	{
		if (string.IsNullOrEmpty(AppVersion))
			AppVersion = this.GetDefaultPropertyString(nameof(AppVersion));
		if (string.IsNullOrEmpty(StorageVersion))
			StorageVersion = this.GetDefaultPropertyString(nameof(StorageVersion));
		IsUseProxy = this.GetDefaultPropertyBool(nameof(IsUseProxy));

		AppXml = new();
		LoadXmlSettings();
		// If file app xml is not exists.
		StoreXmlSettings();
	}

	#endregion

	#region Methods

	public string ToDebugString() => AppXml.ToDebugString();

	public void LoadXmlSettings(Encoding? encoding = null)
	{
		// Use for console app
		if (TgGlobalTools.AppType != TgEnumAppType.Console && TgGlobalTools.AppType != TgEnumAppType.Memory) return;

        if (!File.Exists(TgFileUtils.FileAppXmlSettings)) return;
		using StreamReader streamReader = new(TgFileUtils.FileAppXmlSettings, encoding ?? Encoding.Unicode);
		var xml = streamReader.ReadToEnd();
		if (!string.IsNullOrEmpty(xml))
			AppXml = TgDataFormatUtils.DeserializeFromXml<TgAppXmlModel>(xml);
	}

	public void DefaultXmlSettings(Encoding? encoding = null)
	{
		AppXml.Default();
		StoreXmlSettings(encoding);
	}

	public void StoreXmlSettings(Encoding? encoding = null)
	{
		// Use for console app
		if (TgGlobalTools.AppType != TgEnumAppType.Console && TgGlobalTools.AppType != TgEnumAppType.Memory) return;

		var xml = TgDataFormatUtils.SerializeAsXmlDocument(AppXml, isAddEmptyNamespace: true).InnerXml;
		xml = TgDataFormatUtils.GetPrettyXml(xml);
		using FileStream fileStream = new(TgFileUtils.FileAppXmlSettings, FileMode.Create);
		using StreamWriter streamWriter = new(fileStream, encoding ?? Encoding.Unicode);
		streamWriter.Write(xml);
	}

	/// <summary> Set version from assembly </summary>
	/// <param name="assembly"></param>
	public void SetVersion(Assembly assembly)
	{
		AppVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? string.Empty;
		ushort count = 0, pos = 0;
		foreach (var c in AppVersion)
		{
			if (Equals(c, '.'))
				count++;
			if (count is 3)
				break;
			pos++;
		}
		if (count is 3)
			AppVersion = AppVersion[..pos];
		AppVersion = $"v{AppVersion}";
	}

	#endregion
}
