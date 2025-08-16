// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Services;

/// <summary> Flood control manager for Telegram operations </summary>
public sealed partial class TgFloodControlService : TgDisposable, ITgFloodControlService
{
    #region Public and private fields, properties, constructor

    /// <inheritdoc />
    public int MaxRetryCount { get; } = 5;
    /// <inheritdoc />
    public int WaitFallbackFast { get; } = 1;
    /// <inheritdoc />
    public int WaitFallbackFlood { get; } = 10;
    /// <inheritdoc />
    public int[] WaitTimeOuts { get; } = [15, 30, 45, 60];
    /// <summary> Semaphore to control concurrent access to Telegram API calls </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    [GeneratedRegex(@"FLOOD_WAIT_(\d+)")]
    private static partial Regex RegexFloodWait();

    #endregion

    #region TgDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        _semaphore.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Public and private methods

    /// <inheritdoc />
    public async Task WaitIfFloodAsync(string message)
    {
        var waitSeconds = TryExtractFloodWaitSeconds(message);
        if (waitSeconds <= 0) return;

        await _semaphore.WaitAsync();
        try
        {
#if DEBUG
            Debug.WriteLine($"[FloodControl] Detected FLOOD_WAIT_{waitSeconds}. Waiting...");
#endif
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds + 1));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteWithFloodControlAsync<T>(Func<Task<T>> telegramCall, bool isThrow)
    {
        for (int attempt = 0; attempt < MaxRetryCount; attempt++)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await telegramCall();
            }
            catch (Exception ex)
            {
                var floodWaitSeconds = TryExtractFloodWaitFromException(ex);
                if (floodWaitSeconds > 0)
                {
#if DEBUG
                    Debug.WriteLine($"[FloodControl] FLOOD_WAIT_{floodWaitSeconds} on attempt {attempt + 1}");
#endif
                    await Task.Delay(TimeSpan.FromSeconds(floodWaitSeconds + 1));
                    continue; // retry
                }

                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        if (isThrow)
            throw new TimeoutException($"Exceeded max retry count ({MaxRetryCount}) for Telegram call");

        return default!;
    }

    /// <summary> Extracts flood wait seconds from the given message </summary>
    private int TryExtractFloodWaitSeconds(string message)
    {
        if (string.IsNullOrEmpty(message)) return 0;
        var match = RegexFloodWait().Match(message);
        return match.Success && int.TryParse(match.Groups[1].Value, out var seconds) ? seconds : 0;
    }

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

    #endregion
}
