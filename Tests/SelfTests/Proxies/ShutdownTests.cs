namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Проверяем завершение освобождения настоящего клиента до возврата Dispose
    private static async Task DisposeWaitsForTelegramDisconnectAsync()
    {
        using var fixture = new ClientFixture();
        var uiUpdate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Connection.SetupUpdateStateProxy(_ => uiUpdate.Task);
        try
        {
            await Task.Run(fixture.Connection.Dispose).WaitAsync(TimeSpan.FromSeconds(5));
            Assert(fixture.Connection.Client is null && !fixture.IsSessionOpen, "Dispose вернулся до освобождения Telegram-клиента и файла сессии");
        }
        finally { uiUpdate.TrySetResult(); }
    }

    // Ошибка уже закрытого интерфейса не должна мешать освобождению транспорта
    private static async Task DisposeDoesNotCallClosedUiAsync()
    {
        using var fixture = new ClientFixture();
        var uiCalls = 0;
        fixture.Connection.SetupUpdateStateProxy(_ => { uiCalls++; throw new InvalidOperationException("Интерфейс уже закрыт"); });
        fixture.Connection.SetupAfterClientConnect(() => { uiCalls++; return Task.CompletedTask; });
        await Task.Run(fixture.Connection.Dispose).WaitAsync(TimeSpan.FromSeconds(5));
        fixture.Connection.Dispose();
        Assert(fixture.Connection.Client is null && !fixture.IsSessionOpen && uiCalls == 0,
            "Освобождение клиента обращается к интерфейсу или оставляет сессию открытой");
    }
}
