// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObjectAsync<T>(string value)
    {
        return await Task.Run<T>(() =>
        {
            return JsonSerializer.Deserialize<T>(value, TgJsonSerializerUtils.GetJsonOptions());
        });
    }

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonSerializer.Serialize(value);
        });
    }
}
