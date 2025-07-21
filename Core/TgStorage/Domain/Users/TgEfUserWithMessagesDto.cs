// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User messages DTO </summary>
public sealed partial class TgEfUserWithMessagesDto : TgSensitiveDto
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial TgEfUserDto UserDto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto ChatDto { get; set; } = default!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> MessageDtos { get; set; } = default!;

    public TgEfUserWithMessagesDto() : base()
	{
        DefaultValues();
    }

    public TgEfUserWithMessagesDto(TgEfUserDto userDto, TgEfSourceDto chatDto, List<TgEfMessageDto> messageDtos) : base()
	{
        UserDto = userDto;
        ChatDto = chatDto;
        MessageDtos = [.. messageDtos];
    }

    #endregion

    #region Public and private methods

    public void DefaultValues()
    {
        UserDto = new();
        ChatDto = new();
        MessageDtos = [];
    }

    #endregion
}
