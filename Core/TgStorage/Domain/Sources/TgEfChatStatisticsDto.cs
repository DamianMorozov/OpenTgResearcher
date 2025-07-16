// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> Chat statistics DTO </summary>
public sealed partial class TgEfChatStatisticsDto : TgDtoBase
{
    #region Public and private fields, properties, constructor

    public static readonly DateTimeOffset SafeMinDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset SafeMaxDate = new DateTimeOffset(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);
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


    public TgEfChatStatisticsDto() : base()
	{
        DefaultValues();
    }

    #endregion

    #region Public and private methods

    public void DefaultValues()
    {
        UsersCount = 0;
        BotsCount = 0;
        UserWithCountDtos.Clear();
    }

    #endregion
}
