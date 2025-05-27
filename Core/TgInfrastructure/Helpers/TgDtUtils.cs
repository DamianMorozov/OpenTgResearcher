// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Date time utilities </summary>
public static class TgDtUtils
{
	#region Public and private methods

	/// <summary>
	/// Get DateTime from unix long.
	/// </summary>
	/// <param name="unixDate"></param>
	/// <returns></returns>
	public static DateTime CastLongAsDtOldStyle(long unixDate)
	{
		DateTime dt = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		dt = dt.AddSeconds(unixDate).ToLocalTime();
		return dt;
	}

	public static long CastDtAsLong(DateTime dt) => dt.Ticks;

	public static long CastAsLong(this DateTime dt) => CastDtAsLong(dt);

	public static DateTime CastLongAsDt(long ticks) => new(ticks);

	public static DateTime CastAsDt(this long ticks) => CastLongAsDt(ticks);

    /// <summary> Format last seen ago string </summary>
	public static string FormatLastSeenAgo(TimeSpan lastSeenAgo)
	{
        // Get the moment of the last appearance of the user
        var lastSeenDateTime = DateTime.UtcNow - lastSeenAgo;
		var now = DateTime.UtcNow;

        // If the user has been within the last minute
        if (lastSeenAgo.TotalMinutes < 1)
            return "online just now";

        // If the user has been within the last hour
        if (lastSeenAgo.TotalMinutes < 60)
            return $"was online {Math.Floor(lastSeenAgo.TotalMinutes)} min ago";

        // If the user has been in the last 24 hours
        if (lastSeenAgo.TotalHours < 24 && lastSeenDateTime.Date == now.Date)
            return $"was online at {lastSeenDateTime:HH:mm} today";

        // If the user was yesterday
        if (lastSeenDateTime.Date == now.Date.AddDays(-1))
            return $"was online at {lastSeenDateTime:HH:mm} yesterday";

        // If the user has been in the last week
        if (lastSeenAgo.TotalDays < 7)
            return $"was online at {lastSeenDateTime:HH:mm} on {lastSeenDateTime:dddd}";

        // If the user was a long time ago
        if (lastSeenAgo.TotalDays >= 150)
            return $"was online a long time ago";

        return $"was online at {lastSeenDateTime:HH:mm} on {lastSeenDateTime:yyyy-MM-dd}";
    }

	#endregion
}