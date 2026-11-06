namespace TgBusinessLogic.Services;

/// <summary> Общий учёт активных сетевых операций и последних ста завершений </summary>
public sealed class TgNetworkActivityService
{
    private readonly Lock _gate = new();
    private readonly Dictionary<Guid, TgNetworkOperation> _operations = [];
    private readonly Dictionary<Guid, Action> _cancellations = [];
    private readonly Dictionary<Guid, TaskCompletionSource> _paused = [];

    public IReadOnlyList<TgNetworkOperation> Operations
    {
        get
        {
            using (_gate.EnterScope())
                return _operations.Values.OrderByDescending(x => x.State == TgNetworkOperationState.Running).ThenByDescending(x => x.StartedUtc).ToArray();
        }
    }

    /// <summary> Регистрирует начало реальной операции, сохраняя независимые параллельные действия </summary>
    public void Start(Guid uid, TgNetworkOperationKind kind)
    {
        using (_gate.EnterScope()) _operations.TryAdd(uid, new(uid, kind, DateTime.UtcNow, null, TgNetworkOperationState.Running));
    }

    /// <summary> Уточняет назначение общей загрузки после подготовки токена </summary>
    public void SetKind(Guid uid, TgNetworkOperationKind kind)
    {
        using (_gate.EnterScope())
            if (_operations.TryGetValue(uid, out var item)) _operations[uid] = item with { Kind = kind };
    }

    /// <summary> Связывает операцию с чатом независимо от имени файла, названия и отображения конфиденциальных данных </summary>
    public void SetChat(Guid uid, long chatId, string chatTitle)
    {
        using (_gate.EnterScope())
            if (_operations.TryGetValue(uid, out var item) && item.State == TgNetworkOperationState.Running)
                _operations[uid] = item with { ChatId = chatId, ChatTitle = chatTitle };
    }

    /// <summary> Связывает активный парсинг с его собственным токеном отмены </summary>
    public void RegisterCancellation(Guid uid, Action cancel)
    {
        using (_gate.EnterScope())
        {
            if (!_operations.TryGetValue(uid, out var item) || item.State != TgNetworkOperationState.Running) return;
            _cancellations[uid] = cancel;
            _operations[uid] = item with { CanCancel = true };
        }
    }

    /// <summary> Запрашивает отмену только активного парсинга, сохраняя другие сетевые операции </summary>
    public void CancelParsing()
    {
        using (_gate.EnterScope())
        {
            var active = _operations.Values.Where(x => x.Kind == TgNetworkOperationKind.Parsing && x.State == TgNetworkOperationState.Running && x.CanCancel).ToArray();
            foreach (var item in active)
            {
                if (!_cancellations.TryGetValue(item.Id, out var cancel)) continue;
                _operations[item.Id] = item with { CanCancel = false, IsCancellationRequested = true, IsPaused = false };
                cancel();
                if (_paused.Remove(item.Id, out var pending)) pending.TrySetResult();
            }
        }
    }

    /// <summary> Приостанавливает новые действия парсинга или разрешает продолжение без отмены токена </summary>
    public void ToggleParsingPause()
    {
        using (_gate.EnterScope())
        {
            var active = _operations.Values.Where(x => x.Kind == TgNetworkOperationKind.Parsing && x.State == TgNetworkOperationState.Running && x.CanCancel).ToArray();
            var pause = !active.Any(x => x.IsPaused);
            foreach (var item in active)
            {
                _operations[item.Id] = item with { IsPaused = pause };
                if (pause) _paused.TryAdd(item.Id, new(TaskCreationOptions.RunContinuationsAsynchronously));
                else if (_paused.Remove(item.Id, out var pending)) pending.TrySetResult();
            }
        }
    }

    /// <summary> Асинхронно ожидает продолжения только указанного парсинга, учитывая отмену </summary>
    public async Task WaitIfPausedAsync(Guid uid, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Task pending;
        using (_gate.EnterScope()) pending = _paused.TryGetValue(uid, out var paused) ? paused.Task : Task.CompletedTask;
        await pending.WaitAsync(ct);
        ct.ThrowIfCancellationRequested();
    }

    /// <summary> Сохраняет имя файла и фактически переданные байты без локального пути </summary>
    public void ReportTransfer(Guid uid, string fileName, long transferred, long total)
    {
        using (_gate.EnterScope())
        {
            if (!_operations.TryGetValue(uid, out var item) || item.State != TgNetworkOperationState.Running) return;
            var size = Math.Max(0, total);
            var received = size > 0 ? Math.Clamp(transferred, 0, size) : Math.Max(0, transferred);
            _operations[uid] = item with { Detail = Path.GetFileName(fileName), TransferredBytes = received, TotalBytes = size,
                Progress = size > 0 ? received * 100d / size : null };
        }
    }

    /// <summary> Сохраняет первый запрашиваемый ID порции до ожидания ответа, не меняя процент уже прочитанных сообщений </summary>
    public void ReportParsingRequest(Guid uid, long currentMessageId, long lastMessageId)
    {
        using (_gate.EnterScope())
        {
            if (!_operations.TryGetValue(uid, out var item) || item.State != TgNetworkOperationState.Running
                || item.Kind != TgNetworkOperationKind.Parsing) return;
            _operations[uid] = item with { CurrentMessageId = currentMessageId >= 0 ? currentMessageId : null,
                LastMessageId = lastMessageId > 0 ? lastMessageId : null };
        }
    }

    /// <summary> Отражает чат и продвижение по сообщениям отдельно от процентов скачивания файлов </summary>
    public void ReportParsing(Guid uid, string chatTitle, long current, long total)
    {
        using (_gate.EnterScope())
        {
            if (!_operations.TryGetValue(uid, out var item) || item.State != TgNetworkOperationState.Running) return;
            _operations[uid] = item with { Detail = chatTitle, Progress = total > 0 ? Math.Clamp(current * 100d / total, 0, 100) : null };
        }
    }

    /// <summary> Завершает только активную операцию и ограничивает историю без удаления работающих </summary>
    public void Finish(Guid uid, TgNetworkOperationState state)
    {
        if (state == TgNetworkOperationState.Running) return;
        using (_gate.EnterScope())
        {
            if (!_operations.TryGetValue(uid, out var item) || item.State != TgNetworkOperationState.Running) return;
            _operations[uid] = item with { State = state, FinishedUtc = DateTime.UtcNow, CanCancel = false, IsPaused = false };
            _cancellations.Remove(uid);
            if (_paused.Remove(uid, out var pending)) pending.TrySetResult();
            var expired = _operations.Values.Where(x => x.State != TgNetworkOperationState.Running)
                .OrderByDescending(x => x.FinishedUtc).Skip(100).Select(x => x.Id).ToArray();
            foreach (var id in expired) _operations.Remove(id);
        }
    }

    /// <summary> Выполняет действие, сохраняя завершение, ошибку или отмену и не скрывая исключения </summary>
    public Task RunAsync(TgNetworkOperationKind kind, Func<Task> action, CancellationToken ct = default) => RunAsync(kind, _ => action(), ct);

    /// <summary> Передаёт действию идентификатор для публикации настоящего прогресса </summary>
    public async Task RunAsync(TgNetworkOperationKind kind, Func<Guid, Task> action, CancellationToken ct = default)
    {
        var uid = Guid.NewGuid();
        Start(uid, kind);
        try
        {
            ct.ThrowIfCancellationRequested();
            await action(uid);
            Finish(uid, ct.IsCancellationRequested ? TgNetworkOperationState.Canceled : TgNetworkOperationState.Completed);
        }
        catch (OperationCanceledException) { Finish(uid, TgNetworkOperationState.Canceled); throw; }
        catch { Finish(uid, TgNetworkOperationState.Failed); throw; }
    }

    /// <summary> Очищает только историю, не меняя выполняющиеся операции </summary>
    public void ClearCompleted()
    {
        using (_gate.EnterScope())
            foreach (var uid in _operations.Values.Where(x => x.State != TgNetworkOperationState.Running).Select(x => x.Id).ToArray()) _operations.Remove(uid);
    }
}
