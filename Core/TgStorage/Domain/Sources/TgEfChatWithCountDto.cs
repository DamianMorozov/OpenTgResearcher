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

    public TgEfChatWithCountDto() : base()
	{
        DefaultValues();
    }

    public TgEfChatWithCountDto(TgEfUserDto userDto, TgEfSourceDto chatDto, int countMessages) : base()
	{
        UserDto = userDto;
        ChatDto = chatDto;
        CountMessages = countMessages;
    }

    #endregion

    #region Methods

    public void DefaultValues()
    {
        UserDto = new();
        ChatDto = new();
        CountMessages = 0;
    }

    #endregion
}
