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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectorySizeText))]
    public partial long? DirectorySizeBytes { get; private set; }
    public string DirectorySizeText
    {
        get
        {
            if (DirectorySizeBytes is not { } bytes) return "—";
            var unit = bytes < 1024 ? 0 : bytes < 1024L * 1024 ? 1 : bytes < 1024L * 1024 * 1024 ? 2 : 3;
            var key = unit switch { 0 => "RegistryBytes", 1 => "RegistryKilobytes", 2 => "RegistryMegabytes", _ => "RegistryGigabytes" };
            return $"{bytes / Math.Pow(1024, unit):0.##} {key.GetLocalizedResource()}";
        }
    }
    public string Updated => updated == default ? "—" : updated.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    [ObservableProperty]
    public partial double Progress { get; private set; }
    [ObservableProperty]
    public partial string ProgressText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    /// <summary> Считает объём файлов каталога и подкаталогов вне потока интерфейса </summary>
    public async Task UpdateDirectorySizeAsync(CancellationToken ct = default)
    {
        try
        {
            var bytes = await Task.Run(() => ReadDirectorySize(ct), ct).ConfigureAwait(false);
            await SetDirectorySizeAsync(bytes, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetDirectorySizeAsync(null, ct).ConfigureAwait(false);
        }
    }

    // Публикуем результат через диспетчер без продолжений в уже закрытом контексте окна
    private Task SetDirectorySizeAsync(long? bytes, CancellationToken ct) => TgDesktopUtils.InvokeOnUIThreadAsync(() =>
    {
        if (!ct.IsCancellationRequested) DirectorySizeBytes = bytes;
        return Task.CompletedTask;
    }, ct);

    // Не следуем по ссылкам на каталоги, чтобы не учитывать посторонние файлы и циклы
    private long? ReadDirectorySize(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Directory) || !System.IO.Directory.Exists(Directory)) return null;
            var options = new EnumerationOptions
                { RecurseSubdirectories = true, IgnoreInaccessible = false, AttributesToSkip = System.IO.FileAttributes.ReparsePoint };
            long bytes = 0;
            foreach (var file in new DirectoryInfo(Directory).EnumerateFiles("*", options))
            {
                ct.ThrowIfCancellationRequested();
                bytes += file.Length;
            }
            return bytes;
        }
        catch (UnauthorizedAccessException)
        {
            TgLogUtils.WriteLog($"Размер каталога чата {Source.Id} недоступен: нет прав чтения. Проверьте каталог в настройках чата: {Directory}");
            return null;
        }
        catch (IOException ex)
        {
            TgLogUtils.WriteLog($"Не удалось прочитать размер каталога чата {Source.Id}: {Directory}. {ex.Message}");
            return null;
        }
    }

    /// <summary> Обновляет статус из реальной операции без сетевых запросов и записи в БД </summary>
    public void Refresh(TgNetworkOperation? operation)
    {
        Progress = operation?.Progress ?? TgDataUtils.CalcSourceProgress(Source.Count, Source.FirstId);
        ProgressText = $"{Progress:0.00} %";
        var key = operation is null ? "ChatReady" : operation.State == TgNetworkOperationState.Running
            ? operation.IsCancellationRequested ? "NetworkStateStopping" : operation.IsPaused ? "NetworkStatePaused" : "NetworkStateRunning"
            : $"NetworkState{operation.State}";
        Status = key.GetLocalizedResource();
        OnPropertyChanged(nameof(DirectorySizeText));
    }
}
