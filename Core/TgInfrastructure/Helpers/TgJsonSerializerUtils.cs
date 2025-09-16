namespace TgInfrastructure.Helpers;

public static class TgJsonSerializerUtils
{
	public static JsonSerializerOptions GetJsonOptions() => new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
	};
}
