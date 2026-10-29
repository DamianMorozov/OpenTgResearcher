namespace TgBusinessLogic.Models;

public sealed class TgParseHelper
{
    #region Fields, properties, constructor

    public bool IsAccess { get; set; }
    public TL.Channel? Channel { get; set; }
    public TgEfAppDto? AppDto { get; set; }
    public WTelegram.Types.ChatFullInfo? BotChatFullInfo { get; set; }
    public TgChatCache ChatCache { get; set; } = new();
    public TgForumTopicSettings ForumTopicSettings { get; set; } = new();
    public ConcurrentQueue<TgTaskItem> Tasks { get; set; } = [];
    public int SourceFirstId { get; set; }
    public int SourceLastId { get; set; }
    public TgMessageSettings MessageSettings { get; set; } = new();

    public long CurrentChatId { get => MessageSettings.CurrentChatId; set => MessageSettings.CurrentChatId = value; }
    public int TasksCount => Tasks.Count;
    public bool TasksIsEmpty => Tasks.IsEmpty;
    public int TasksMessage { get; private set; }
    public int TasksMedia { get; private set; }

    #endregion

    #region Methods

    public TgTaskItem AddTask(Task task, int messageId)
    {
        var item = new TgTaskItem(task, CurrentChatId, messageId);
        Tasks.Enqueue(item);
        return item;
    }

    public bool TryDequeueTask(out TgTaskItem? item)
    {
        if (Tasks.TryDequeue(out var dequeued))
        {
            if (dequeued.TaskType == TgTaskType.Message)
                TasksMessage = Math.Max(0, TasksMessage - 1);
            else if (dequeued.TaskType == TgTaskType.Media)
                TasksMedia = Math.Max(0, TasksMedia - 1);

            item = dequeued;
            return true;
        }

        item = null;
        return false;
    }

    public void ClearTasks()
    {
        while (Tasks.TryDequeue(out _)) { }
        TasksMessage = 0;
        TasksMedia = 0;
    }

    public void SetTaskType(long messageId, TgTaskType taskType)
    {
        foreach (var taskItem in Tasks)
        {
            if (taskItem.ChatId == CurrentChatId && taskItem.MessageId == messageId)
            {
                if (taskItem.TaskType == TgTaskType.Default)
                {
                    taskItem.SetTaskType(taskType);

                    if (taskType == TgTaskType.Message)
                        TasksMessage++;
                    else if (taskType == TgTaskType.Media)
                        TasksMedia++;
                }
                break;
            }
        }
    }

    #endregion
}
