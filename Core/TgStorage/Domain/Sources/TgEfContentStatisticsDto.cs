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


    public TgEfContentStatisticsDto() : base()
	{
        DefaultValues();
    }

    #endregion

    #region Methods

    public void DefaultValues()
    {
        ImagesCount = 0;
        AudiosCount = 0;
        VideosCount = 0;
    }

    #endregion
}
