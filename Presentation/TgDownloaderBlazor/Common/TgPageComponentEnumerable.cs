// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Common;

public abstract class TgPageComponentEnumerable<ITgDto, TEntity> : TgPageComponentBase //where TDto : ITgDto<TDto, TEntity>
{
	#region Public and private fields, properties, constructor

    protected RadzenDataGrid<ITgDto> Grid { get; set; } = new();
	protected string SearchString { get; set; } = string.Empty;
	protected IEnumerable<ITgDto> Dtos { get; set; } = [];
	protected int ItemsCount { get; set; }
    protected string ItemsSummaryString => $"{TgLocale.FoundRows}: {ItemsCount}";

	#endregion

	#region Public and private methods

	~TgPageComponentEnumerable()
	{
		Grid.Dispose();
	}

    #endregion
}