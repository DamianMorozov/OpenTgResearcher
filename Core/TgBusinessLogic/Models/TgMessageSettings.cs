// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Models;

/// <summary> Message settings </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgMessageSettings
{
    #region Public and private fields, properties, constructor

    /// <summary> Thread number </summary>
    public int ThreadNumber { get; set; }
    /// <summary> Current message ID </summary>
    public int CurrentMessageId { get; set; }
    /// <summary> Parent message ID </summary>
    public int ParentMessageId { get; set; }
    /// <summary> Current chat ID </summary>
    public long CurrentChatId { get; set; }
    /// <summary> Parent chat ID </summary>
    public long ParentChatId { get; set; }
    /// <summary> Thrown flag for Telegram calls </summary>
    public bool IsThrow { get; set; }

    #endregion

    #region Public and private methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public override string ToString() => ToDebugString();

    /// <summary> Create a deep copy of the current TgMessageDownloadSettings instance </summary>
    public TgMessageSettings Clone()
    {
        var copy = new TgMessageSettings
        {
            ThreadNumber = ThreadNumber,
            CurrentMessageId = CurrentMessageId,
            ParentMessageId = ParentMessageId,
            CurrentChatId = CurrentChatId,
            ParentChatId = ParentChatId,
            IsThrow = IsThrow
        };
        return copy;
    }

    #endregion
}
