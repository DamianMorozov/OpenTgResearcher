namespace OpenTgResearcherDesktop.Helpers;

public sealed class TgRuntimeHelper
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

	public static bool IsMSIX
	{
		get
		{
			var length = 0;

			return GetCurrentPackageFullName(ref length, null) != 15700L;
		}
	}
}
