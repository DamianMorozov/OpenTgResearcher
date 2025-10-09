namespace TgBusinessLogic.Contracts;

// <summary> Interface for flood control service in Telegram operations </summary>
public interface ITgFloodControlService : IDisposable
{
    /// <summary> Maximum number of retries </summary>
    public int MaxRetryCount { get; }
    /// <summary> Fast delay between retries </summary>
    public int WaitFallbackFast { get; }
    /// <summary> Delay between retries </summary>
    public int WaitFallbackFlood { get; }
    /// <summary> Wait timeouts in seconds for retrying Telegram API calls </summary>
    public int[] WaitSeconds { get; }

    /// <summary> Waits if flood control message is detected in logs </summary>
    public Task WaitIfFloodAsync(string message, CancellationToken ct);
    /// <summary> Executes Telegram call with flood control retry logic </summary>
    public Task<T> ExecuteWithFloodControlAsync<T>(Func<CancellationToken, Task<T>> telegramCall, bool isThrow, CancellationToken ct);
    /// <summary> Extracts flood wait seconds from the given message </summary>
    public int TryExtractFloodWaitSeconds(string message);
    /// <summary> Check flood </summary>
    public bool IsFlood(string message);
    /// <summary> Gets the size, in bytes, of the current data chunk being processed </summary>
    public int CurrentChunkSize { get; }
    /// <summary> Registers a flood hit event </summary>
    public void RegisterFloodHit();
    /// <summary> Registers a successful operation event </summary>
    public void RegisterSuccess();
}
