// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
    public int[] WaitTimeOuts { get; }

    /// <summary> Waits if flood control message is detected in logs </summary>
    public Task WaitIfFloodAsync(string message);
    /// <summary> Executes Telegram call with flood control retry logic </summary>
    public Task<T> ExecuteWithFloodControlAsync<T>(Func<Task<T>> telegramCall, bool isThrow);
    /// <summary> Extracts flood wait seconds from the given message </summary>
    public int TryExtractFloodWaitSeconds(string message);
    /// <summary> Check flood </summary>
    public bool IsFlood(string message);
}
