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
    public partial int CountMessages { get; set; } = default!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> Messages { get; set; } = [];
    public TgEfChatWithCountDto() : base() => Default();

    public TgEfChatWithCountDto(TgEfUserDto userDto, TgEfSourceDto chatDto, int countMessages) : base()
	{
        UserDto = userDto;
        ChatDto = chatDto;
        CountMessages = countMessages;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Default()
    {
        UserDto = new();
        ChatDto = new();
        CountMessages = 0;
        Messages = [];
    }

    #endregion
}
