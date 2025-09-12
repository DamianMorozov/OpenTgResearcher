namespace TgInfrastructure.Helpers;

/// <summary> Data format utilities </summary>
public static class TgDataFormatUtils
{
	#region Methods - ISerializable

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
		var xmlSerializer = XmlSerializer.FromTypes([typeof(T)])[0];
		// The T object must have properties with { get; set; }.
		using StringWriter stringWriter = new();
		switch (isAddEmptyNamespace)
		{
			case true:
			{
				XmlSerializerNamespaces emptyNamespaces = new();
				emptyNamespaces.Add(string.Empty, string.Empty);
				using var xmlWriter = XmlWriter.Create(stringWriter, GetXmlWriterSettings());
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
		var xmlString = SerializeAsXmlString(item, isAddEmptyNamespace);
		var bytes = Encoding.Unicode.GetBytes(xmlString);
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
		var xmlSerializer = XmlSerializer.FromTypes([typeof(T)])[0];
		if (xmlSerializer is null)
			return new();
		var obj = xmlSerializer.Deserialize(new MemoryStream(Encoding.Unicode.GetBytes(xml)));
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
		var regexPattern = Regex.Escape(mask)
			// Replace ? with . (match any character) and * with .* (match any number of characters)
			.Replace("\\?", ".")
			.Replace("\\*", ".*");
		// Check if the file name matches the regex pattern
		return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
	}

	public static Guid ParseStringToGuid(string uid) => Guid.TryParse(uid, out var guid) ? guid : Guid.Empty;

	public static string ParseGuidToString(Guid uid) => uid.ToString().Replace("-", "");

	public static string TrimStringEnd(string value, int len = 30) =>
		string.IsNullOrEmpty(value) ? string.Empty : value.Length <= len ? value : value[..len];

    public static string TrimStringEndOrNewLine(string value, int len = 30)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Find the index of the first line break
        int newLineIndex = value.IndexOfAny(['\r', '\n']);

        // If there is a line break and it is before len, trim to it
        int trimIndex = (newLineIndex >= 0 && newLineIndex < len) ? newLineIndex : len;

        // If the string length is less than trimIndex, return the whole string
        if (value.Length <= trimIndex)
            return value;

        return value.Substring(0, trimIndex);
    }


    public static string GetFormatString(string? value, int len = 30) =>
		string.IsNullOrEmpty(value) ? string.Empty : value.PadRight(len)[..len];

	public static string GetFormatStringWithStrongLength(string? value, int len = 30)
	{
		if (string.IsNullOrEmpty(value))
			return new('.', len);
		value = RemoveNonPrintableCharacters(value, " ");
		var result = value.Length > len ? value[..len] : $"{value}{new string('.', len - value.Length)}";
		return result;
	}

	public static string RemoveNonPrintableCharacters(string input, string replacement)
	{
		if (string.IsNullOrEmpty(input)) return input;
		// Replace all types of line breaks
		input = input
			.Replace("\r\n", replacement)	// Windows
			.Replace("\n", replacement)		// Unix
			.Replace("\r", replacement)		// Mac
			.Replace("[", "_")		// AnsiConsole
			.Replace("]", "_")      // AnsiConsole
			.Replace("{", "_")		// AnsiConsole
			.Replace("}", "_")      // AnsiConsole
			;
		var sb = new StringBuilder();
		foreach (char c in input)
		{
			// Check if the character is a control character (unprintable)
			if (!char.IsControl(c))
			{
				sb.Append(c);
			}
		}
		return sb.ToString();
	}

	public static string GetDtFormat(DateTime dt) => $"{dt:yyyy-MM-dd HH:mm:ss}";

	public static string GetDateFormat(DateTime dt) => $"{dt:yyyy-MM-dd}";

	public static string GetTimeFormat(DateTime dt) => $"{dt:HH:mm:ss}";

	#endregion
}
