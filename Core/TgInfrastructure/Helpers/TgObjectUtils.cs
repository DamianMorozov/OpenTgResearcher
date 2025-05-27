// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Object utilities </summary>
public static class TgObjectUtils
{
	public static string ToDebugString(this object obj)
	{
		if (obj is null)
			throw new ArgumentException(nameof(obj));
		var stringProperties = obj.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(prop => prop.PropertyType == typeof(string));
		return string.Join(" | ", stringProperties.Select(prop => $"{prop.Name}: {prop.GetValue(obj)}"));
	}
}
