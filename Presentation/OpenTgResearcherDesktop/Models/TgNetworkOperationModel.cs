namespace OpenTgResearcherDesktop.Models;

/// <summary> Локализованная строка операции с учётом скрытия названий чатов и файлов </summary>
public sealed record TgNetworkOperationModel
{
    public string Title { get; }
    public string State { get; }
    public string Started { get; }
    public string Duration { get; }
    public string Icon { get; }
    public string Detail { get; }
    public bool HasDetail => !string.IsNullOrEmpty(Detail);
    public double Progress { get; }
    public bool HasProgress { get; }
    public string ProgressText { get; }
    public string MessageReading { get; }
    public string Caption => string.Join(" · ", new[] { Title, MessageReading, Detail }.Where(x => !string.IsNullOrEmpty(x)));
    public string ProgressCompact { get; }

    /// <summary> Подготавливает безопасные подписи снимка для отображения в списке </summary>
    public TgNetworkOperationModel(TgNetworkOperation operation, DateTime nowUtc, bool showSensitiveData = true)
    {
        Title = $"NetworkKind{operation.Kind}".GetLocalizedResource();
        State = (operation.State == TgNetworkOperationState.Running
            ? operation.IsCancellationRequested ? "NetworkStateStopping" : operation.IsPaused ? "NetworkStatePaused" : "NetworkStateRunning"
            : $"NetworkState{operation.State}").GetLocalizedResource();
        Detail = string.IsNullOrEmpty(operation.Detail) ? string.Empty : showSensitiveData ? operation.Detail : "**********";
        MessageReading = operation.Kind != TgNetworkOperationKind.Parsing || !operation.CurrentMessageId.HasValue ? string.Empty
            : operation.LastMessageId.HasValue
                ? string.Format(CultureInfo.CurrentCulture, "NetworkParsingMessageIds".GetLocalizedResource(), operation.CurrentMessageId, operation.LastMessageId)
                : string.Format(CultureInfo.CurrentCulture, "NetworkParsingMessageId".GetLocalizedResource(), operation.CurrentMessageId);
        HasProgress = operation.Progress.HasValue;
        Progress = operation.Progress ?? 0;
        ProgressText = !HasProgress ? string.Empty : operation.Kind == TgNetworkOperationKind.Download
            ? string.Format(CultureInfo.CurrentCulture, "NetworkDownloadProgress".GetLocalizedResource(), Progress,
                TgFileUtils.GetFileSizeAsString(operation.TransferredBytes), TgFileUtils.GetFileSizeAsString(operation.TotalBytes))
            : string.Format(CultureInfo.CurrentCulture, "NetworkParsingProgress".GetLocalizedResource(), Progress);
        ProgressCompact = !HasProgress ? "—" : operation.Kind == TgNetworkOperationKind.Download
            ? $"{Progress:0.00} % · {TgFileUtils.GetFileSizeAsString(operation.TransferredBytes)} / {TgFileUtils.GetFileSizeAsString(operation.TotalBytes)}"
            : $"{Progress:0.00} %";
        Started = operation.StartedUtc.ToLocalTime().ToString("HH:mm:ss");
        var elapsed = (operation.FinishedUtc ?? nowUtc) - operation.StartedUtc;
        Duration = $"{(int)Math.Max(0, elapsed.TotalHours):00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        Icon = operation.Kind switch
        {
            TgNetworkOperationKind.Connection => "connect", TgNetworkOperationKind.Disconnection => "disconnect",
            TgNetworkOperationKind.Updates => "app-update", TgNetworkOperationKind.Download => "download",
            TgNetworkOperationKind.Parsing => "activity", TgNetworkOperationKind.Proxies => "proxies", _ => "network"
        };
    }
}
