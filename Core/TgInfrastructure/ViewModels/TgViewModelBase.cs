// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.ViewModels;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgViewModelBase : ObservableRecipient, ITgViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial bool IsLoad { get; set; }

    #endregion

    #region Public and private methods

    public virtual string ToDebugString() => $"{TgDataUtils.GetIsLoad(IsLoad)}";

    #endregion
}