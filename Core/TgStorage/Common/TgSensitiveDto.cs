// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Sensitive DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSensitiveDto : TgDtoBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string SensitiveData { get; set; } = "**********";
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; set; }

    public TgSensitiveDto() : base()
	{
		//
	}

    #endregion

    #region Public and private methods

    //

	#endregion
}