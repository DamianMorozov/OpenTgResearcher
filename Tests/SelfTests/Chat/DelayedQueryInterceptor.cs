namespace OpenTgResearcherDesktop.ChatSelfTests;

internal sealed class DelayedQueryInterceptor : DbCommandInterceptor
{
    internal static bool Enabled { get; set; }
    internal static int HoldNext;
    internal static Task? Release { get; set; }

    /// <summary> Удерживает настоящий SQL-запрос, чтобы выявить параллельное использование контекста </summary>
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken ct = default)
    {
        if (Release is { } release && Interlocked.Decrement(ref HoldNext) >= 0) await release.WaitAsync(ct);
        if (Enabled) await Task.Delay(30, ct);
        return result;
    }
}
