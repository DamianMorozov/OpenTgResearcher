namespace OpenTgResearcherDesktop.ChatSelfTests;

internal sealed class DelayedQueryInterceptor : DbCommandInterceptor
{
    internal static bool Enabled { get; set; }

    /// <summary> Удерживает настоящий SQL-запрос, чтобы выявить параллельное использование контекста </summary>
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken ct = default)
    {
        if (Enabled) await Task.Delay(30, ct);
        return result;
    }
}
