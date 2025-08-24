// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base common </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract class TgCommonBase : ITgDebug
{
    #region Methods

    public virtual string ToDebugString() => throw new NotImplementedException(TgConstants.UseOverrideMethod);

    #endregion
}