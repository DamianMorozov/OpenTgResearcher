namespace TgInfrastructure.Helpers;

/// <summary> String utilities </summary>
public static class TgStringUtils
{
    #region Methods

    /// <summary> Normalize TG name </summary>
    public static string NormalizedTgName(string name, bool isAddAt = true)
    {
        if (name.StartsWith("https://t.me/"))
            name = name.Substring(13, name.Length - 13);
        if (isAddAt && !name.StartsWith('@'))
            name = $"@{name}";
        if (!isAddAt && name.StartsWith('@'))
            name = name.Substring(1, name.Length - 1);
        return name;
    }

    /// <summary> Normalize names from string to list of names </summary>
    public static List<string> NormalizedTgNames(string names, bool isAddAt = true)
    {
        var separators = new char[] { ',', ';', ' ' };
        var list = names.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(name => NormalizedTgName(name.Trim(), isAddAt).ToLowerInvariant())
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

    /// <summary> Ensure that the given file name has a single extension, removing any existing extensions </summary>
    public static string EnsureSingleExtension(string name)
    {
        // Extract existing extension if any
        var ext = Path.GetExtension(name);
        var baseName = Path.GetFileNameWithoutExtension(name);

        // Normalize extension to include leading dot
        if (!string.IsNullOrEmpty(ext))
            ext = ext.StartsWith('.') ? ext : "." + ext;
        else
            ext = string.Empty;

        // Remove trailing dots from base name
        baseName = baseName.TrimEnd('.');

        return baseName + ext;
    }
    
    /// <summary> Ensure that the given file name has a single extension, replacing any existing extension with the specified one </summary>
    public static string EnsureSingleExtension(string name, string ext)
    {
        // Normalize extension like ".mp4"
        if (string.IsNullOrWhiteSpace(ext)) return Path.GetFileName(name);

        var normalizedExt = ext.StartsWith('.') ? ext : "." + ext;

        // Strip existing extension and rebuild
        var baseName = Path.GetFileNameWithoutExtension(name);
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = Path.GetFileName(name);

        // Collapse trailing dots or spaces in base name
        baseName = baseName.Trim().TrimEnd('.');

        return baseName + normalizedExt;
    }

    /// <summary> Generate a safe file name from the given name and extension </summary>
    public static string SanitizeForFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        // Collapse whitespace
        return string.Join(" ", cleaned.Split([' '], StringSplitOptions.RemoveEmptyEntries));
    }

    #endregion
}
