using TL;

namespace TgBusinessLogic.Services;

/// <summary> Flood control manager for Telegram operations </summary>
public sealed partial class TgFloodControlService : TgDisposable, ITgFloodControlService
{
    #region Fields, properties, constructor

    private readonly Lock _gate = new();
    private DateTime _blockedUntilUtc;
    /// <inheritdoc />
    public int MaxRetryCount { get; } = 5;
    /// <inheritdoc />
    public int WaitFallbackFast { get; } = 1;
    /// <inheritdoc />
    public int WaitFallbackFlood { get; } = 10;
    /// <inheritdoc />
    public int[] WaitSeconds { get; } = [15, 30, 45, 60];
    [GeneratedRegex(@"FLOOD_WAIT_(\d+)")]
    public static partial Regex RegexFloodWait();

    private const int MinChunk = 1;
    private const int MaxChunk = 20;
    private const int DefaultChunk = 10;
    private int _currentChunk = DefaultChunk;
    private int _floodHits = 0;
    private int _successHits = 0;
    public int CurrentChunkSize => Volatile.Read(ref _currentChunk);

    #endregion

    #region TgDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        // Защитная пауза не владеет соединениями и токенами парсеров
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task WaitIfFloodAsync(string message, CancellationToken ct)
    {
        var waitSeconds = TryExtractFloodWaitSeconds(message);
        if (waitSeconds <= 0) return;

        ExtendFloodWait(waitSeconds);
        try
        {
            await WaitForFloodCooldownAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Ожидание этого чата завершается, общая защитная пауза сохраняется
            return;
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteWithFloodControlAsync<T>(Func<CancellationToken, Task<T>> telegramCall, bool isThrow, CancellationToken ct)
    {
        for (int attempt = 0; attempt < MaxRetryCount; attempt++)
        {
            // Ожидаем только ограничение Telegram, не удерживая другие запросы на время передачи файла
            await WaitForFloodCooldownAsync(ct);
            ct.ThrowIfCancellationRequested();
            try
            {
                return await telegramCall(ct);
            }
            catch (Exception ex)
            {
                // Отмена текущего парсинга не затрагивает запросы соседних чатов
                if (ct.IsCancellationRequested)
                    return await Task.FromCanceled<T>(ct);

                var floodWaitSeconds = TryExtractFloodWaitFromException(ex);
                if (floodWaitSeconds > 0)
                {
#if DEBUG
                    Debug.WriteLine($"[FloodControl] FLOOD_WAIT_{floodWaitSeconds} on attempt {attempt + 1}");
#endif
                    ExtendFloodWait(floodWaitSeconds + 1);
                    await WaitForFloodCooldownAsync(ct);
                    continue; // Повторяем запрос после паузы
                }

                throw;
            }
        }

        if (isThrow)
            throw new TimeoutException($"Превышено число повторных запросов Telegram: {MaxRetryCount}");

        return default!;
    }

    // Продлеваем общую защитную паузу, сохраняя наиболее поздний срок из ответов Telegram
    private void ExtendFloodWait(int seconds)
    {
        using (_gate.EnterScope())
        {
            var until = DateTime.UtcNow.AddSeconds(seconds);
            if (until > _blockedUntilUtc) _blockedUntilUtc = until;
        }
    }

    // Перепроверяем срок после ожидания, если другой запрос продлил FLOOD_WAIT
    private async Task WaitForFloodCooldownAsync(CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            TimeSpan remaining;
            using (_gate.EnterScope()) remaining = _blockedUntilUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero) return;
            await Task.Delay(remaining, ct);
        }
    }

    /// <inheritdoc />
    public int TryExtractFloodWaitSeconds(string message)
    {
        if (string.IsNullOrEmpty(message)) return 0;
        var match = RegexFloodWait().Match(message);
        var waitSeconds = match.Success && int.TryParse(match.Groups[1].Value, out var seconds) ? seconds + 1 : 0;
        if (waitSeconds > 0)
        {
            RegisterFloodHit();
        }
        else
        {
            RegisterSuccess();
        }
        return waitSeconds;
    }

    /// <inheritdoc />
    public bool IsFlood(string message) => TryExtractFloodWaitSeconds(message) > 0;

    /// <summary> Extract flood wait seconds from an exception or its inner exceptions </summary>
    private int TryExtractFloodWaitFromException(Exception ex)
    {
        // Direct RpcException
        if (ex is RpcException rpcEx && rpcEx.Code == 420)
            return TryExtractFloodWaitSeconds(rpcEx.Message);

        // AggregateException
        if (ex is AggregateException aggEx)
        {
            foreach (var inner in aggEx.InnerExceptions)
            {
                var wait = TryExtractFloodWaitFromException(inner);
                if (wait > 0) return wait;
            }
        }

        // Inner exception
        if (ex.InnerException != null)
            return TryExtractFloodWaitFromException(ex.InnerException);

        // Fallback: check message
        return TryExtractFloodWaitSeconds(ex.Message);
    }

    /// <inheritdoc />
    public void RegisterFloodHit()
    {
        using (_gate.EnterScope())
        {
            _floodHits++;
            _successHits = 0;
            // Уменьшаем порцию после двух ограничений подряд
            if (_floodHits >= 2)
            {
                _currentChunk = Math.Max(MinChunk, _currentChunk / 2);
                _floodHits = 0;
            }
        }
    }

    /// <inheritdoc />
    public void RegisterSuccess()
    {
        using (_gate.EnterScope())
        {
            _successHits++;
            // Увеличиваем порцию после пяти успешных обращений
            if (_successHits >= 5)
            {
                _currentChunk = Math.Min(MaxChunk, _currentChunk + 20);
                _successHits = 0;
            }
        }
    }

    #endregion
}
