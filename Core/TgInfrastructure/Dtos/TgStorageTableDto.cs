namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageTableDto : ObservableRecipient
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial string Name { get; private set; } = string.Empty;
	[ObservableProperty]
	public partial int RecordsCount { get; set; }

    public TgStorageTableDto(string name, int recordsCount)
    {
        Name = name;
        RecordsCount = recordsCount;
    }

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    #endregion
}
