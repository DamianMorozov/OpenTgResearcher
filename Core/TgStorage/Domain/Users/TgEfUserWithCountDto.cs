// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User statistics DTO </summary>
public sealed partial class TgEfUserWithCountDto : TgDtoBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial TgEfUserDto UserDto { get; set; } = default!;
    [ObservableProperty]
    public partial int Count { get; set; }


    public TgEfUserWithCountDto() : base()
	{
        DefaultValues();
    }

    #endregion

    #region Public and private methods

    public void DefaultValues()
    {
        UserDto = new();
        Count = 0;
    }

    #endregion
}
