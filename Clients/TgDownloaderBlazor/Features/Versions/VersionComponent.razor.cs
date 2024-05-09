﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Features.Versions;

public partial class VersionComponent : TgPageComponentEnumerable<TgEfVersionEntity>
{
	#region Public and private fields, properties, constructor

	private TgEfVersionRepository VersionRepository { get; } = new(TgEfUtils.EfContext);

	#endregion

	#region Public and private methods

	protected override async Task OnInitializedAsync()
    {
	    await base.OnInitializedAsync();
	    if (!IsBlazorLoading)
		    return;

	    await using TgEfContext? efContext = await EfFactory.CreateDbContextAsync();
	    if (!AppSettings.AppXml.IsExistsEfStorage)
	    {
		    IsBlazorLoading = false;
		    return;
	    }

	    Items = (await VersionRepository.GetListAsync(0, isNoTracking: false))
			.Items.OrderByDescending(x => x.Version).ToList();
        ItemsCount = await VersionRepository.GetCountAsync();

        IsBlazorLoading = false;
	}

    #endregion
}