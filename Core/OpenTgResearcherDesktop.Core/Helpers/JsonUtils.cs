// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Core.Helpers;

public static class JsonUtils
{
    public static T ToObject<T>(string value) => JsonSerializer.Deserialize<T>(value, TgJsonSerializerUtils.GetJsonOptions());

    public static string Stringify(object value) => JsonSerializer.Serialize(value);
}
