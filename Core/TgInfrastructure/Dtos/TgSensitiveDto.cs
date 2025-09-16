namespace TgInfrastructure.Dtos;

/// <summary> Sensitive DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSensitiveDto : TgDtoBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string SensitiveData { get; set; } = "**********";
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; set; } = true;

    public TgSensitiveDto() : base()
    {
        //
    }

    #endregion

    #region Methods

    //

    #endregion
}
