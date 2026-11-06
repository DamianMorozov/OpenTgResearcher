namespace OpenTgResearcherDesktop.Models;

/// <summary> Строка реестра с сохранёнными данными чата и независимым снимком сетевой активности </summary>
public sealed partial class TgChatRegistryRow(TgEfSourceLiteDto source, string directory, int messagesCount, int filesCount, DateTime updated) : ObservableObject
{
    public TgEfSourceLiteDto Source { get; } = source;
    public string Directory { get; } = directory;
    public int MessagesCount { get; } = messagesCount;
    public int FilesCount { get; } = filesCount;
    public string Title => string.IsNullOrWhiteSpace(Source.Title) ? Source.Id.ToString() : Source.Title;
    public string Identity => string.IsNullOrWhiteSpace(Source.UserName) ? $"ID {Source.Id}" : $"ID {Source.Id} · @{Source.UserName}";
    public string MessagesText => MessagesCount.ToString("N0");
    public string FilesText => FilesCount.ToString("N0");
    public string Updated => updated == default ? "—" : updated.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    [ObservableProperty]
    public partial double Progress { get; private set; }
    [ObservableProperty]
    public partial string ProgressText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    /// <summary> Обновляет статус из реальной операции без сетевых запросов и записи в БД </summary>
    public void Refresh(TgNetworkOperation? operation)
    {
        Progress = operation?.Progress ?? TgDataUtils.CalcSourceProgress(Source.Count, Source.FirstId);
        ProgressText = $"{Progress:0.00} %";
        var key = operation is null ? "ChatReady" : operation.State == TgNetworkOperationState.Running
            ? operation.IsCancellationRequested ? "NetworkStateStopping" : operation.IsPaused ? "NetworkStatePaused" : "NetworkStateRunning"
            : $"NetworkState{operation.State}";
        Status = key.GetLocalizedResource();
    }
}
