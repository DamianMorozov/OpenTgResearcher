// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Utils;

/// <summary> Data format utilities </summary>
public static class TgDataFormatUtils
{
	#region Public and private methods - ISerializable

	public static XmlReaderSettings GetXmlReaderSettings() => new()
	{
		ConformanceLevel = ConformanceLevel.Document
	};

	public static XmlWriterSettings GetXmlWriterSettings() => new()
	{
		ConformanceLevel = ConformanceLevel.Document,
		OmitXmlDeclaration = false,
		Encoding = Encoding.Unicode,
		Indent = true,
		IndentChars = "\t"
	};

	public static string SerializeAsXmlString<T>(T item, bool isAddEmptyNamespace) where T : new()
	{
		// Don't use it.
		// XmlSerializer xmlSerializer = new(typeof(T));
		// Use it.
		XmlSerializer? xmlSerializer = XmlSerializer.FromTypes([typeof(T)])[0];
		// The T object must have properties with { get; set; }.
		using StringWriter stringWriter = new();
		switch (isAddEmptyNamespace)
		{
			case true:
			{
				XmlSerializerNamespaces emptyNamespaces = new();
				emptyNamespaces.Add(string.Empty, string.Empty);
				using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, GetXmlWriterSettings());
				xmlSerializer?.Serialize(xmlWriter, item, emptyNamespaces);
				xmlWriter.Flush();
				xmlWriter.Close();
				break;
			}
			default:
				xmlSerializer?.Serialize(stringWriter, item);
				break;
		}
		return stringWriter.ToString();
	}

	public static XmlDocument SerializeAsXmlDocument<T>(T item, bool isAddEmptyNamespace) where T : new()
	{
		XmlDocument xmlDocument = new();
		string xmlString = SerializeAsXmlString(item, isAddEmptyNamespace);
		byte[] bytes = Encoding.Unicode.GetBytes(xmlString);
		using MemoryStream memoryStream = new(bytes);
		memoryStream.Flush();
		memoryStream.Seek(0, SeekOrigin.Begin);
		xmlDocument.Load(memoryStream);
		return xmlDocument;
	}

	public static T DeserializeFromXml<T>(string xml) where T : new()
	{
		// Don't use it.
		// XmlSerializer xmlSerializer = new(typeof(T));
		// Use it.
		XmlSerializer? xmlSerializer = XmlSerializer.FromTypes([typeof(T)])[0];
		if (xmlSerializer is null)
			return new();
		object? obj = xmlSerializer.Deserialize(new MemoryStream(Encoding.Unicode.GetBytes(xml)));
		if (obj is null)
			return new();
		return (T)obj;
	}

	/// <summary>
	/// Get pretty formatted XML string.
	/// </summary>
	/// <param name="xml"></param>
	public static string GetPrettyXml(string xml) =>
		string.IsNullOrEmpty(xml) ? string.Empty : XDocument.Parse(xml).ToString();

	public static bool CheckFileAtMask(string name, string mask)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(mask))
			return false;

		// Escape special regex characters in the mask
		string regexPattern = Regex.Escape(mask)
			// Replace ? with . (match any character) and * with .* (match any number of characters)
			.Replace("\\?", ".")
			.Replace("\\*", ".*");
		// Check if the file name matches the regex pattern
		return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
	}

	public static Guid ParseStringToGuid(string uid) => Guid.TryParse(uid, out Guid guid) ? guid : Guid.Empty;

	public static string ParseGuidToString(Guid uid) => uid.ToString().Replace("-", "");

	public static string TrimStringEnd(string value, int len = 30) =>
		string.IsNullOrEmpty(value) ? string.Empty : value.Length <= len ? value : value[..len];

	public static string GetFormatString(string? value, int len = 30) =>
		string.IsNullOrEmpty(value) ? string.Empty : value.PadRight(len)[..len];

	public static string GetFormatStringWithStrongLength(string? value, int len = 30)
	{
		if (string.IsNullOrEmpty(value))
			return new string('.', len);
		var result = value.Length > len ? value[..len] : $"{value}{new string('.', len - value.Length)}";
		return result;
	}

	public static string GetDtFormat(DateTime dt) => $"{dt:yyyy-MM-dd HH:mm:ss}";

	public static string GetDateFormat(DateTime dt) => $"{dt:yyyy-MM-dd}";

	public static string GetTimeFormat(DateTime dt) => $"{dt:HH:mm:ss}";

	#endregion
}