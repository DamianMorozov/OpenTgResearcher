// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> Content statistics DTO </summary>
public sealed partial class TgEfContentStatisticsDto : TgDtoBase
{
    #region Public and private fields, properties, constructor

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

    #region Public and private methods

    public void DefaultValues()
    {
        ImagesCount = 0;
        AudiosCount = 0;
        VideosCount = 0;
    }

    #endregion
}
