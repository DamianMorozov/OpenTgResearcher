namespace TgInfrastructure.ViewModels;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgViewModelBase : ObservableRecipient, ITgViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial bool IsLoad { get; set; }

    #endregion

    #region Methods

    public virtual string ToDebugString() => $"{TgDataUtils.GetIsLoad(IsLoad)}";

    #endregion
}
