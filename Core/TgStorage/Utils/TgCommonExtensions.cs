// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Utils;

/// <summary>
/// Common extensions.
/// </summary>
public static class TgCommonExtensions
{
	#region Private methods

	private static object? GetPropertyDefaultValueCore<T>(this T item, string name)
	{
		if (item is null)
			return null;
		var attributes = TypeDescriptor.GetProperties(item)[name]?.Attributes;
		var attribute = attributes?[typeof(DefaultValueAttribute)];
		if (attribute is DefaultValueAttribute defaultValueAttribute)
			return defaultValueAttribute.Value;
		return null;
	}

	#endregion

	#region Public methods

	public static TResult? GetDefaultPropertyGeneric<TResult>(this object item, string name) =>
		GetPropertyDefaultValueCore(item, name) is TResult value ? value : default;

	#endregion

	#region Public methods - Checked

	public static Guid GetDefaultPropertyGuid(this object item, string name) => 
		GetPropertyDefaultValueCore(item, name) is Guid value ? value : Guid.Empty;

	public static bool GetDefaultPropertyBool(this object item, string name)
	{
		return GetPropertyDefaultValueCore(item, name) is bool value ? value : default;
	}

	public static string GetDefaultPropertyString(this object item, string name) => 
		GetPropertyDefaultValueCore(item, name)?.ToString() ?? string.Empty;

	public static short GetDefaultPropertyShort(this object item, string name)
	{
		var value = GetPropertyDefaultValueCore(item, name);
		return value is not null ? Convert.ToInt16(value) : default;
	}

	public static ushort GetDefaultPropertyUshort(this object item, string name)
	{
		var value = GetPropertyDefaultValueCore(item, name);
		return value is not null ? Convert.ToUInt16(value) : default;
	}

	public static int GetDefaultPropertyInt(this object item, string name)
	{
		return GetPropertyDefaultValueCore(item, name) is int value ? value : default;
	}

	public static uint GetDefaultPropertyUint(this object item, string name)
	{
		return GetPropertyDefaultValueCore(item, name) is int value ? (uint)value : default;
	}

	public static long GetDefaultPropertyLong(this object item, string name)
	{
		return GetPropertyDefaultValueCore(item, name) is int value ? (long)value : default;
	}

	public static ulong GetDefaultPropertyUlong(this object item, string name)
	{
		return GetPropertyDefaultValueCore(item, name) is int value ? (ulong)value : default;
	}

	public static DateTime GetDefaultPropertyDateTime(this object item, string name) =>
		GetPropertyDefaultValueCore(item, name) is string value
			? DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
			: DateTime.MinValue;

	#endregion
}