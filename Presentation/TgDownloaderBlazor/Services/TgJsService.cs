// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Services;

public class TgJsService
{
    #region Public and private fields, properties, constructor

    [Inject] private IJSRuntime JsRuntime { get; init; }
    [Inject] private NotificationService NotificationService { get; set; }
    private IJSObjectReference Module { get; set; } = null!;

    public TgJsService(IJSRuntime jsRuntime, NotificationService notificationService)
    {
        JsRuntime = jsRuntime;
        NotificationService = notificationService;

        var task = InitializeModuleAsync();
        task.Wait();
    }

    #endregion

    #region Public and private methods

    private async Task InitializeModuleAsync()
    {
        Module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/appUtils.js");
    }

    public async Task CopyTextToClipboard(string text)
    {
        await Module.InvokeVoidAsync("copyToClipboard", text);
    }

    public async Task RedirectBack()
    {
        await Module.InvokeVoidAsync("navigateBackOrHome");
    }

    public async Task OpenLink(string url)
    {
        await Task.Delay(1);
        await JsRuntime.InvokeVoidAsync("open", url, "_blank");
        NotificationService.Notify(new NotificationMessage
        {
	        Severity = NotificationSeverity.Info,
			Summary = TgLocaleHelper.Instance.UrlOpened,
	        //SummaryContent = ns => $@"< RadzenText TextStyle = ""TextStyle.H6"" > Custom summary: < br /> @DateTime.Now </ RadzenText >",
	        Detail = url,
	        //DetailContent = ns => "< RadzenButton Text = \"Clear\" Click = \"@(args => ns.Messages.Clear())\" />"
	        Duration = 5_000,
		});
    }

	#endregion
}
