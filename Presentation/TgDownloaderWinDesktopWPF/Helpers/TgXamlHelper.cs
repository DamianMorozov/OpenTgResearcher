// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.Helpers;

/// <summary>
/// XAML helper.
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
internal sealed class TgXamlHelper : ITgHelper
{
	#region Design pattern "Lazy Singleton"

	private static TgXamlHelper _instance = null!;
	public static TgXamlHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

	#endregion

	#region Public and private methods

    public string ToDebugString() => TgLocaleHelper.Instance.UseOverrideMethod;

    #endregion
}