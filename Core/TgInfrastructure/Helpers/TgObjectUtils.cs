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
