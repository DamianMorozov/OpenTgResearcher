// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgBugBountyScopeDto : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string Icon { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Entry { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LinkUrl { get; set; } = string.Empty;

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    #endregion
}
