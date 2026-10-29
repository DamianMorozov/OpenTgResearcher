namespace TgBusinessLogic.Models;

/// <summary> Telegram task item </summary>
public sealed class TgTaskItem(Task task, long chatId, int messageId)
{
    #region Fields, properties, constructor

    public Guid Uid { get; } = Guid.NewGuid();
    public Task Task { get; } = task ?? throw new ArgumentNullException(nameof(task));
    public TgTaskType TaskType { get; set; } = TgTaskType.Default;
    public long ChatId { get; set; } = chatId;
    public int MessageId { get; set; } = messageId;

    #endregion

    #region Methods

    public void SetTaskType(TgTaskType taskType) => TaskType = taskType;

    #endregion
}
