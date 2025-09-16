namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgBugBountyRuleDto : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string Icon { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    #endregion
}
