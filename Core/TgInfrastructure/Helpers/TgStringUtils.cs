// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> String utilities </summary>
public static class TgStringUtils
{
    /// <summary> Normilize TG name </summary>
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

    /// <summary> Normilize names from string to list of names </summary>
    public static List<string> NormilizeTgNames(string names)
    {
        var separators = new char[] { ',', ';', ' ' };
        var list = names.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim().ToLower())
            .ToList();
        return list;
    }
}
