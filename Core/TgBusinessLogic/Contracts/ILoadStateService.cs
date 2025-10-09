using System.ComponentModel;

namespace TgBusinessLogic.Contracts;

/// <summary> Load state service to track loading states in the application </summary>
public interface ILoadStateService : INotifyPropertyChanged
{
    /// <summary> Is memory processing </summary>
    public bool IsMemoryProcessing { get; }
    /// <summary> Is storage processing </summary>
    public bool IsStorageProcessing { get; }
    /// <summary> Is online processing </summary>
    public bool IsOnlineProcessing { get; }
    /// <summary> Is locator processing </summary>
    public bool IsLocatorProcessing { get; }
    /// <summary> Is download processing </summary>
    public bool IsDownloadProcessing { get; }
    /// <summary> Is flood log processing </summary>
    public bool IsFloodLogProcessing { get; }
    /// <summary> Is display sensitive data </summary>
    public bool IsDisplaySensitiveData { get; }
    /// <summary> Is online ready </summary>
    public bool IsOnlineReady { get; }
    /// <summary> Sensitive data mask </summary>
    public string SensitiveData { get; }
    /// <summary> Cancellation token source for Memory </summary>
    public CancellationTokenSource? MemoryCts { get; }
    /// <summary> Cancellation token for Memory </summary>
    public CancellationToken MemoryToken { get; }
    /// <summary> Cancellation token source for Storage </summary>
    public CancellationTokenSource? StorageCts { get; }
    /// <summary> Cancellation token for Storage </summary>
    public CancellationToken StorageToken { get; }
    /// <summary> Cancellation token source for online </summary>
    public CancellationTokenSource? OnlineCts { get; }
    /// <summary> Cancellation token for online </summary>
    public CancellationToken OnlineToken { get; }
    /// <summary> Cancellation token source for locator </summary>
    public CancellationTokenSource? LocatorCts { get; }
    /// <summary> Cancellation token for locator </summary>
    public CancellationToken LocatorToken { get; }
    /// <summary> Cancellation token source for download </summary>
    public CancellationTokenSource? DownloadCts { get; }
    /// <summary> Cancellation token for download </summary>
    public CancellationToken DownloadToken { get; }
    /// <summary> Cancellation token source for flood log </summary>
    public CancellationTokenSource? FloodLogCts { get; }
    /// <summary> Cancellation token for flood log </summary>
    public CancellationToken FloodLogToken { get; }

    /// <summary> Set online ready </summary>
    public void SetIsOnlineReady(bool isReady);
    /// <summary> Set display sensitive </summary>
    public void SetIsDisplaySensitiveData(bool isOn);

    /// <summary> Prepare memory token </summary>
    public Task PrepareMemoryTokenAsync(Guid uid);
    /// <summary> Prepare storage token </summary>
    public Task PrepareStorageTokenAsync(Guid uid);
    /// <summary> Prepare online token </summary>
    public Task PrepareOnlineTokenAsync(Guid uid);
    /// <summary> Prepare locator token </summary>
    public Task PrepareLocatorTokenAsync(Guid uid);
    /// <summary> Prepare download token </summary>
    public Task PrepareDownloadTokenAsync(Guid uid);
    /// <summary> Prepare flood log token </summary>
    public Task PrepareFloodLogTokenAsync(Guid uid);
    /// <summary> Stop all processing </summary>
    public void StopHardAllProcessing();
    /// <summary> Stop memory processing </summary>
    public void StopHardMemoryProcessing();
    /// <summary> Stop storage processing </summary>
    public void StopHardStorageProcessing();
    /// <summary> Stop online processing </summary>
    public void StopHardOnlineProcessing();
    /// <summary> Stop locator processing </summary>
    public void StopHardLocatorProcessing();
    /// <summary> Stop download processing </summary>
    public void StopHardDownloadProcessing();
    /// <summary> Stop flood log processing </summary>
    public void StopHardFloodLogProcessing();
    /// <summary> Check memory processing </summary>
    public void StopSoftMemoryProcessing(Guid uid);
    /// <summary> Check storage processing </summary>
    public void StopSoftStorageProcessing(Guid uid);
    /// <summary> Check online processing </summary>
    public void StopSoftOnlineProcessing(Guid uid);
    /// <summary> Check locator processing </summary>
    public void StopSoftLocatorProcessing(Guid uid);
    /// <summary> Check download processing </summary>
    public void StopSoftDownloadProcessing(Guid uid);
    /// <summary> Check flood log processing </summary>
    public void StopSoftFloodLogProcessing(Guid uid);
}
