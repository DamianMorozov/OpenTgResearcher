namespace TgStorage.Utils;

public static class TgEfExtensions
{
    /// <summary> Case-insensitive Contains for EF Core (works with SQLite and other providers) </summary>
    public static bool ContainsIgnoreCase(this string? source, string value)
    {
        // Return false if source or value is null or empty
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return false;

        // Use EF.Functions.Like to ensure translation to SQL
        return EF.Functions.Like(source!.ToLower(), $"%{value.ToLower()}%");
    }
}
