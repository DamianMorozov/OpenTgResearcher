namespace OpenTgResearcherDesktopCore.Helpers;

public static class JsonUtils
{
    public static T ToObject<T>(string value) => JsonSerializer.Deserialize<T>(value, TgJsonSerializerUtils.GetJsonOptions());

    public static string Stringify(object value) => JsonSerializer.Serialize(value);
}
