namespace TgBusinessLogic.Services;

public abstract partial class TgConnectClientBase
{
    // Контекст наследуется задачами одного парсинга, но не соседними вызовами клиента
    private readonly AsyncLocal<ParsingSession?> _parsingSession = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource> _parsingChats = new();
    private int _isStoppingParsing;

    private TgDownloadSettingsViewModel? DownloadSettings => _parsingSession.Value?.Settings ?? _downloadSettings;

    // Резервируем чат до первого ожидания и освобождаем только после завершения всех его задач
    private async Task RunParsingSessionAsync(TgDownloadSettingsViewModel settings)
    {
        if (Volatile.Read(ref _isStoppingParsing) != 0) return;
        var chatId = settings.SourceVm.Dto.Id;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_parsingChats.TryAdd(chatId, completion)) return;
        var previous = _parsingSession.Value;
        try
        {
            if (Volatile.Read(ref _isStoppingParsing) != 0) return;
            using var session = new ParsingSession(settings);
            _parsingSession.Value = session;
            await ParseChatCoreAsync(session.Settings);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
        finally
        {
            _parsingSession.Value = previous;
            _parsingChats.TryRemove(chatId, out _);
            completion.TrySetResult();
        }
    }

    /// <summary> Останавливает все чаты и ожидает завершения их задач перед освобождением клиента </summary>
    public async Task StopParsingAsync(CancellationToken ct = default)
    {
        try
        {
            Interlocked.Exchange(ref _isStoppingParsing, 1);
            var completions = _parsingChats.Values.Select(x => x.Task).ToArray();
            TgLogUtils.WriteLog($"Завершение парсинга: ожидание чатов — {completions.Length}");
            LoadStateService.StopHardDownloadProcessing();
            await Task.WhenAll(completions).WaitAsync(ct);
            TgLogUtils.WriteLog("Завершение парсинга: все чаты остановлены");
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    // Каждый чат владеет настройками, кешем и репозиториями; подключение Telegram и FLOOD_WAIT остаются общими
    private sealed class ParsingSession : IDisposable
    {
        private TgStorageService? _storage;
        public ITgStorageService Storage => _storage ??= new TgStorageService();
        public TgDownloadSettingsViewModel Settings { get; set; }
        public IEnumerable<TgEfFilterDto> Filters { get; set; } = [];
        public IFusionCache Cache { get; } = new FusionCache(new FusionCacheOptions());
        public TgBufferCacheHelper<TgEfMessageDto> Messages { get; }
        public TgBufferCacheHelper<TgEfMessageRelationDto> Relations { get; }
        public TgBufferCacheHelper<TgEfSourceDto> Chats { get; }
        public TgBufferCacheHelper<TgEfStoryDto> Stories { get; }
        public TgBufferCacheHelper<TgEfUserDto> Users { get; }
        public TgBufferCacheHelper<TgParticipantDto> Participants { get; }

        public ParsingSession(TgDownloadSettingsViewModel settings)
        {
            Settings = settings.CreateParsingSnapshot();
            Messages = new(Cache, TgCacheUtils.GetCacheKeyMessagePrefix(), dto => $"{dto.SourceId}:{dto.Id}");
            Relations = new(Cache, TgCacheUtils.GetCacheKeyMessageRelationPrefix(),
                dto => $"{dto.ParentSourceId}:{dto.ParentMessageId}:{dto.ChildSourceId}:{dto.ChildMessageId}");
            Chats = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix(), dto => $"{dto.Id}");
            Stories = new(Cache, TgCacheUtils.GetCacheKeyStoryPrefix(), dto => $"{dto.Id}");
            Users = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), dto => $"{dto.Id}");
            Participants = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), dto => $"{dto.Id}");
        }

        /// <summary> Освобождает только ресурсы завершившегося парсинга </summary>
        public void Dispose()
        {
            Messages.Dispose(); Relations.Dispose(); Chats.Dispose(); Stories.Dispose(); Users.Dispose(); Participants.Dispose();
            Cache.Dispose(); _storage?.Dispose();
        }
    }
}
