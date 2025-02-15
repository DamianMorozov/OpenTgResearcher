// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Features.Client;

public sealed partial class ClientComponent : TgPageComponentEnumerable<TgEfAppDto, TgEfAppEntity>
{
	#region Public and private fields, properties, constructor

	private TgEfAppDto Dto { get; set; } = null!;
	private TgEfAppRepository AppRepository { get; } = new();

	#endregion

	#region Public and private methods

	protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (!IsBlazorLoading)
	        return;

        if (!AppSettings.AppXml.IsExistsEfStorage)
        {
	        IsBlazorLoading = false;
	        return;
		}

		Dtos = await AppRepository.GetListDtosAsync(0, 0);
        ItemsCount = await AppRepository.GetCountAsync();
        
        await OnClientLoad();
        IsBlazorLoading = false;
    }

	private async Task GridLoadData(LoadDataArgs args)
    {
	    await Task.Delay(1);
		try
        {
            //var result = await TgStorageService.GetApps(filter: $@"(contains(UID,""{SearchString}"") or contains(API_HASH,""{search}"") or contains(PHONE_NUMBER,""{search}"") or contains(PROXY_UID,""{search}"")) and {(string.IsNullOrEmpty(args.Filter) ? "true" : args.Filter)}", orderby: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
            //switch (typeof(TEntity))
            //{
            //    case var cls when cls == typeof(TgEfAppEntity):
            //Items = efContext.AppsRepo.GetEnumerable(0).Cast<TEntity>();
            //ItemsCount = efContext.AppsRepo.GetCount();

            //var query = efContext.Apps.AsNoTracking().AsQueryable();
            //// Filter via the Where method
            //if (!string.IsNullOrEmpty(args.Filter))
            //{
            //    //query = query.Where(args.Filter);
            //}
            //// Sort via the OrderBy method
            //if (!string.IsNullOrEmpty(args.OrderBy))
            //{
            //    //query = query.OrderBy(args.OrderBy);
            //}
            //// Perform paging via Skip and Take.
            //Items = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
            //ItemsCount = query.Count();

            //        break;
            //    case var cls when cls == typeof(TgEfFilterEntity):
            //        Items = efContext.FilterRepo.GetEnumerable(0).Cast<TEntity>();
            //        ItemsCount = efContext.FilterRepo.GetCount();
            //        break;
            //    case var cls when cls == typeof(TgEfMessageEntity):
            //        Items = efContext.MessageRepo.GetEnumerable(0).Cast<TEntity>();
            //        ItemsCount = efContext.MessageRepo.GetCount();
            //        break;
            //    case var cls when cls == typeof(TgEfProxyEntity):
            //        Items = efContext.ProxyRepo.GetEnumerable(0).Cast<TEntity>();
            //        ItemsCount = efContext.ProxyRepo.GetCount();
            //        break;
            //    case var cls when cls == typeof(TgEfSourceEntity):
            //        Items = efContext.SourceRepo.GetEnumerable(0).Cast<TEntity>();
            //        ItemsCount = efContext.SourceRepo.GetCount();
            //        break;
            //    case var cls when cls == typeof(TgEfVersionEntity):
            //        Items = efContext.VersionRepo.GetEnumerable(0).Cast<TEntity>();
            //        ItemsCount = efContext.VersionRepo.GetCount();
            //        break;
            //}
            IsBlazorLoading = false;
        }
        catch (Exception)
        {
            NotificationService.Notify(new() { Severity = NotificationSeverity.Error, Summary = "Error", Detail = "Unable to load Apps" });
        }
    }

    private async Task Search(ChangeEventArgs args)
    {
		await Task.Delay(1);
		SearchString = $"{args.Value}";
        await Grid.GoToPage(0);
        await Grid.Reload();
    }

	//private async Task AddButtonClick(MouseEventArgs args)
	//{
	//    await DialogService.OpenAsync<AddApp>("Add App", null);
	//    await Grid.Reload();
	//}

	//private async Task EditRow(DataGridRowMouseEventArgs<TgEfAppEntity> args)
	//{
	//    await DialogService.OpenAsync<EditApp>("Edit App", new Dictionary<string, object> { { "UID", args.Data.Uid } });
	//    await Grid.Reload();
	//}

	//private async Task GridDeleteButtonClick(MouseEventArgs args, TgEfAppEntity app)
	//{
	//    try
	//    {
	//        if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
	//        {
	//            var deleteResult = await TgStorageService.DeleteApp(uid: app.Uid);

	//            if (deleteResult != null)
	//            {
	//                await Grid.Reload();
	//            }
	//        }
	//    }
	//    catch (Exception ex)
	//    {
	//        NotificationService.Notify(new NotificationMessage
	//        {
	//            Severity = NotificationSeverity.Error,
	//            Summary = $"Error",
	//            Detail = $"Unable to delete App"
	//        });
	//    }
	//}

	//private async Task ExportClick(RadzenSplitButtonItem args)
	//{
	//    if (args?.Value == "csv")
	//    {
	//        await TgStorageService.ExportAppsToCSV(new Query
	//        {
	//            Filter = $@"{(string.IsNullOrEmpty(Grid.Query.Filter) ? "true" : Grid.Query.Filter)}",
	//            OrderBy = $"{Grid.Query.OrderBy}",
	//            Expand = "",
	//            Select = string.Join(",", Grid.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
	//        }, "Apps");
	//    }

	//    if (args == null || args.Value == "xlsx")
	//    {
	//        await TgStorageService.ExportAppsToExcel(new Query
	//        {
	//            Filter = $@"{(string.IsNullOrEmpty(Grid.Query.Filter) ? "true" : Grid.Query.Filter)}",
	//            OrderBy = $"{Grid.Query.OrderBy}",
	//            Expand = "",
	//            Select = string.Join(",", Grid.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
	//        }, "Apps");
	//    }
	//}

	private async Task ClientConnect()
	{
        await Task.Delay(1);

		await OnClientSave(false);

		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			//if (!TgSqlUtils.GetValidXpLite(Item).IsValid)
			// return;
			//await TgClient.ConnectSessionAsync(proxyVm ?? ProxyVm);
			await Task.Delay(1);
			//await TgClient.ConnectSessionAsync(efContext.ProxyRepo.CreateNew());
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Info,
				Summary = TgLocale.MenuClientMessage,
				Detail = TgLocale.MenuClientConnect
			});
		});
	}

	private async Task ClientDisconnect()
	{
		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			await Task.Delay(1);
			await TgClient.DisconnectAsync();
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Info,
				Summary = TgLocale.MenuClientMessage,
				Detail = TgLocale.MenuClientDisconnect
			});
		});
	}

	public async Task OnClientLoad()
	{
		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			await Task.Delay(1);
			var item = await AppRepository.GetFirstItemAsync();
			Dto = new TgEfAppDto().GetDto(item);
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Info,
				Summary = TgLocale.MenuClientMessage,
				Detail = TgLocale.Load
			});
		});
	}

	public async Task OnClientSave(bool isNotificationService)
	{
		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			await Task.Delay(1);
			var item = Dto.GetEntity();
			await AppRepository.SaveAsync(item);
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			if (isNotificationService)
				NotificationService.Notify(new()
				{
					Severity = NotificationSeverity.Info,
					Summary = TgLocale.MenuClientMessage,
					Detail = TgLocale.Save
				});
		});

		await OnClientLoad();
	}

	public async Task OnClientClear()
	{
		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			await Task.Delay(1);
			Dto = new();
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Info,
				Summary = TgLocale.MenuClientMessage,
				Detail = TgLocale.Clear
			});
		});
	}

	public async Task OnClientEmpty()
	{
		await TgBlazorUtils.RunFuncAsync(async () =>
		{
			await Task.Delay(1);
			//await efContext.AppsRepo.DeleteAllItemsAsync();
		}, message =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Error,
				Summary = message,
				Detail = TgLocale.Exception
			});
		}, () =>
		{
			NotificationService.Notify(new()
			{
				Severity = NotificationSeverity.Info,
				Summary = TgLocale.MenuClientMessage,
				Detail = TgLocale.Empty
			});
		});
	}

	#endregion
}