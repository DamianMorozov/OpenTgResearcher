namespace TgStorage.Domain.Sources;

/// <summary> Chat with count DTO </summary>
public sealed partial class TgEfChatWithCountDto : TgDtoBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial TgEfUserDto UserDto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto ChatDto { get; set; } = default!;
    [ObservableProperty]
    public partial int MessagesCount { get; set; } = default!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> Messages { get; set; } = [];
    public TgEfChatWithCountDto() : base() => Default();

    public TgEfChatWithCountDto(TgEfUserDto userDto, TgEfSourceDto chatDto, int messagesCount) : base()
	{
        UserDto = userDto;
        ChatDto = chatDto;
        MessagesCount = messagesCount;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Default()
    {
        UserDto = new();
        ChatDto = new();
        MessagesCount = 0;
        Messages = [];
    }

    #endregion
}
