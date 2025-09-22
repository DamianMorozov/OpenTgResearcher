namespace TgStorage.Domain.Users;

/// <summary> User statistics DTO </summary>
public sealed partial class TgEfUserWithCountDto : TgDtoBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial TgEfUserDto UserDto { get; set; } = default!;
    [ObservableProperty]
    public partial int Count { get; set; }


    public TgEfUserWithCountDto() : base() => DefaultValues();

    #endregion

    #region Methods

    public void DefaultValues()
    {
        UserDto = new();
        Count = 0;
    }

    #endregion
}
