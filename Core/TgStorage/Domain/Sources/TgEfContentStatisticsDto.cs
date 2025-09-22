namespace TgStorage.Domain.Users;

/// <summary> Content statistics DTO </summary>
public sealed partial class TgEfContentStatisticsDto : TgDtoBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial int ImagesCount { get; set; }
    [ObservableProperty]
    public partial int AudiosCount { get; set; }
    [ObservableProperty]
    public partial int VideosCount { get; set; }


    public TgEfContentStatisticsDto() : base() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Default()
    {
        ImagesCount = 0;
        AudiosCount = 0;
        VideosCount = 0;
    }

    #endregion
}
