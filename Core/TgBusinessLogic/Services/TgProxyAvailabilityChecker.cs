namespace TgBusinessLogic.Services;

/// <summary> Результат проверки TCP-подключения к серверу прокси без авторизации в Telegram </summary>
public sealed record TgProxyAvailabilityResult(bool IsAvailable, long ElapsedMilliseconds);

/// <summary> Проверяет доступность сервера и порта прокси, не изменяя активное подключение </summary>
public static class TgProxyAvailabilityChecker
{
    /// <summary> Открывает и освобождает TCP-соединение с ограничением времени ожидания </summary>
    public static async Task<TgProxyAvailabilityResult> CheckAsync(TgEfProxyDto proxy, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(proxy.HostName) || proxy.Port == 0)
                throw new InvalidDataException("Для проверки прокси необходимы сервер и ненулевой порт");
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            deadline.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));
            using var client = new System.Net.Sockets.TcpClient();
            var started = System.Diagnostics.Stopwatch.GetTimestamp();
            await client.ConnectAsync(proxy.HostName, proxy.Port, deadline.Token).ConfigureAwait(false);
            return new(IsAvailable: true, ElapsedMilliseconds: (long)System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, $"Ошибка проверки TCP-доступности прокси {proxy.Uid}");
            return new(IsAvailable: false, ElapsedMilliseconds: 0);
        }
    }
}
