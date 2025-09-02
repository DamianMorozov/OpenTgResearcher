// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

// <summary> Data utilities </summary>
public static partial class TgDataUtils
{
    #region Fields, properties, constructor

    [GeneratedRegex(@"\s+")]
    private static partial Regex RegexMultipleSpace();

    #endregion

    #region Methods

    public static string GetIsAutoUpdate(bool isAutoUpdate) => $"{(isAutoUpdate ? "<Auto update>" : "<Not update>")}";
	
    public static string GetIsFlag(bool isFlag, string positive, string negative) => $"{(isFlag ? positive : negative)}";

	public static string GetIsEnabled(bool isEnabled) => $"{(isEnabled ? "<Enabled>" : "<Disabled>")}";

    public static string GetIsExists(bool isExists) => $"{(isExists ? "<Exist>" : "<Not exist>")}";

    public static string GetIsExistsDb(bool isExistsDb) => $"{(isExistsDb ? "<Exist DB>" : "<Non-existent DB>")}";

    public static string GetIsLoad(bool isLoad) => $"{(isLoad ? "<Loaded>" : "<Not loaded>")}";

    public static string GetIsUseProxy(bool isUseProxy) => $"{(isUseProxy ? "<Use proxy>" : "<Not use proxy>")}";

    public static string GetIsReady(bool isReady) => $"{(isReady ? "<Ready>" : "<Not ready>")}";

    public static string GetIsEmpty(bool isReady) => $"{(isReady ? "<Empty>" : "<Not empty>")}";

    public static double CalcSourceProgress(long count, long current) =>
        count is 0 ? 0 : (double)(current * 100) / count;

    public static string GetLongString(long current) =>
        current > 999 ? $"{current:### ###}" : $"{current:###}";

    public static Version? GetTrimVersion(Version? version)
    {
        if (version is null) return null;
        var versionStr = version.ToString();
        var lastDotIndex = versionStr.LastIndexOf('.');
        if (lastDotIndex < 0) return version;
        versionStr = versionStr[..lastDotIndex];
        return Version.Parse(versionStr);
    }

    /// <summary> Clear TG peer from artefacts </summary>
    public static string ClearTgPeer(string input)
    {
	    if (input.StartsWith("https://t.me/"))
		    input = input.Replace("https://t.me/", string.Empty);
	    else if (input.StartsWith('@'))
		    input = input.Replace("@", string.Empty);
	    return input;
    }

    public static string CleanString(string input)
    {
	    if (string.IsNullOrEmpty(input))
		    return input;
	    // Remove HTML-tags and &nbsp;
	    var cleaned = Regex.Replace(input, "<[^>]*>|&nbsp;", string.Empty);
	    // Remove control characters
	    var sb = new StringBuilder();
	    foreach (char c in cleaned)
	    {
		    if (!char.IsControl(c))
			    sb.Append(c);
	    }
	    cleaned = sb.ToString();
	    // Replace multiple spaces by one
	    cleaned = RegexMultipleSpace().Replace(cleaned, " ");
	    // Trim
	    return cleaned.Trim();
    }

    #endregion
}
