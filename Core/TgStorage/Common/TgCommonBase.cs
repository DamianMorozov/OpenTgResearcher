namespace TgStorage.Common;

/// <summary> Base common </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract class TgCommonBase : ITgDebug
{
    #region Methods

    public virtual string ToDebugString() => throw new NotImplementedException(TgConstants.UseOverrideMethod);

    #endregion
}
