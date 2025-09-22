namespace TgStorage.Domain.Users;

/// <summary> User with messages DTO </summary>
public sealed partial class TgEfUserWithMessagesDto : TgDtoBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial TgEfUserDto UserDto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto ChatDto { get; set; } = default!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> MessageDtos { get; set; } = default!;

    public TgEfUserWithMessagesDto() : base() => Default();

    public TgEfUserWithMessagesDto(TgEfUserDto userDto, TgEfSourceDto chatDto, List<TgEfMessageDto> messageDtos) : base()
	{
        UserDto = userDto;
        ChatDto = chatDto;
        MessageDtos = [.. messageDtos];
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Default()
    {
        UserDto = new();
        ChatDto = new();
        MessageDtos = [];
    }

    #endregion
}
