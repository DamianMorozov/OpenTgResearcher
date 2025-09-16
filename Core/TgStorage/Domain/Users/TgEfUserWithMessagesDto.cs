namespace TgStorage.Domain.Users;

/// <summary> User messages DTO </summary>
public sealed partial class TgEfUserWithMessagesDto : TgSensitiveDto
{
    #region Fields, properties, constructor

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

    #region Methods

    public void DefaultValues()
    {
        UserDto = new();
        ChatDto = new();
        MessageDtos = [];
    }

    #endregion
}
