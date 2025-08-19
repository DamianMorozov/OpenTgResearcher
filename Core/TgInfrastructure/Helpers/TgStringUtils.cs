// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> String utilities </summary>
public static class TgStringUtils
{
    /// <summary> Normalize TG name </summary>
    public static string NormilizeTgName(string name, bool isAddAt = true)
    {
        if (name.StartsWith("https://t.me/"))
            name = name.Substring(13, name.Length - 13);
        if (isAddAt && !name.StartsWith("@"))
            name = $"@{name}";
        if (!isAddAt && name.StartsWith("@"))
            name = name.Substring(1, name.Length - 1);
        return name;
    }

    /// <summary> Normalize names from string to list of names </summary>
    public static List<string> NormilizeTgNames(string names, bool isAddAt = true)
    {
        var separators = new char[] { ',', ';', ' ' };
        var list = names.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(name => NormilizeTgName(name.Trim(), isAddAt).ToLower())
            .ToList();
        return list;
    }

    /// <summary> Format the chat link </summary>
    public static (string, string) FormatChatLink(string chatUserName, long chatId, int messageId) =>
        !string.IsNullOrEmpty(chatUserName) 
        ? (TgLocaleHelper.Instance.MessageLink, $"https://t.me/{chatUserName}/{messageId}") 
        : (TgLocaleHelper.Instance.MessageLink, $"https://t.me/c/{chatId}/{messageId}");

    /// <summary> Format the chat link </summary>
    public static (string, string) FormatChatLink(string chatUserName, long chatId) =>
        !string.IsNullOrEmpty(chatUserName) 
        ? (TgLocaleHelper.Instance.ChatLink, $"https://t.me/{chatUserName}") 
        : (TgLocaleHelper.Instance.ChatLink, $"https://t.me/c/{chatId}");

    /// <summary> Format the user link </summary>
    public static (string, string) FormatUserLink(string userName, long userId, string printName) => 
        string.IsNullOrEmpty(userName) 
            ? ($"{TgLocaleHelper.Instance.FromUserId}: {userId}", string.Empty)  
            : ($"{TgLocaleHelper.Instance.FromUser}: {(!string.IsNullOrEmpty(printName) ? printName : userName)}", $"https://t.me/{userName}");
}
