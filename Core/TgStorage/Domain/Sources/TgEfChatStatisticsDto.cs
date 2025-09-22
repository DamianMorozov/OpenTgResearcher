namespace TgStorage.Domain.Users;

/// <summary> Chat statistics DTO </summary>
public sealed partial class TgEfChatStatisticsDto : TgDtoBase
{
    #region Fields, properties, constructor

    public static readonly DateTimeOffset SafeMinDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset SafeMaxDate = new(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);
    [ObservableProperty]
    public partial DateTimeOffset? DtStart { get; set; }
    [ObservableProperty]
    public partial DateTimeOffset? DtEnd { get; set; }
    [ObservableProperty]
    public partial int UsersCount { get; set; }
    [ObservableProperty]
    public partial int BotsCount { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserWithCountDto> UserWithCountDtos { get; set; } = new ObservableCollection<TgEfUserWithCountDto>();


    public TgEfChatStatisticsDto() : base() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Default()
    {
        UsersCount = 0;
        BotsCount = 0;
        UserWithCountDtos.Clear();
    }

    #endregion
}
