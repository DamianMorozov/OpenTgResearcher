// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.Utils;

/// <summary>
/// Desktop utils.
/// </summary>
public static class TgDesktopUtils
{
	#region Public and private fields, properties, constructor

	public static TgClientViewModel TgClientVm { get; } = new();
    public static TgDashboardViewModel TgDashboardVm { get; } = new();
    public static TgFiltersViewModel TgFiltersVm { get; } = new();
    public static TgItemProxyViewModel TgItemProxyVm { get; } = new();
    public static TgItemSourceViewModel TgItemSourceVm { get; } = new();
	public static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	public static TgProxiesViewModel TgProxiesVm { get; } = new();
    public static TgSettingsViewModel TgSettingsVm { get; } = new();
    public static TgSourcesViewModel TgSourcesVm { get; } = new();
    public static TgDownloadsViewModel TgDownloadsVm{ get; } = new();
    public static TgConstants TgConst { get; } = new();

    #endregion

	#region Public and private methods

	/// <summary>
	/// Set client state update.
	/// </summary>
	public static void SetupClient()
    {
		TgGlobalTools.ConnectClient.SetupUpdateStateConnect(TgClientVm.UpdateStateConnectAsync);
		TgGlobalTools.ConnectClient.SetupUpdateStateProxy(TgClientVm.UpdateStateProxyAsync);
		TgGlobalTools.ConnectClient.SetupUpdateStateSource(TgClientVm.UpdateStateSourceAsync);
		TgGlobalTools.ConnectClient.SetupUpdateStateMessage(TgClientVm.UpdateStateMessageAsync);
		TgGlobalTools.ConnectClient.SetupUpdateStateException(TgClientVm.UpdateStateExceptionAsync);
		TgGlobalTools.ConnectClient.SetupUpdateStateExceptionShort(TgClientVm.UpdateStateExceptionShortAsync);
		TgGlobalTools.ConnectClient.SetupAfterClientConnect(TgClientVm.AfterClientConnectAsync);
		TgGlobalTools.ConnectClient.SetupGetClientDesktopConfig(TgClientVm.ConfigClientDesktop);
    }

    #endregion

    #region Public and private methods

    public static void RunAction(TgPageViewModelBase viewModel, Action action, bool isUpdateLoad)
    {
        void Job()
        {
            if (isUpdateLoad)
                viewModel.IsLoad = true;
            TgClientVm.Exception.Clear();
            action();
        }

        void JobFinally()
        {
            viewModel.IsLoad = false;
        }

        try
        {
            if (viewModel.Dispatcher.CheckAccess())
            {
                Job();
            }
            else
            {
                viewModel.Dispatcher.Invoke(Job);
            }
        }
        catch (Exception ex)
        {
            if (viewModel.Dispatcher.CheckAccess())
                TgClientVm.Exception.Set(ex);
            else
                viewModel.Dispatcher.Invoke(() => { TgClientVm.Exception.Set(ex); });
        }
        finally
        {
            if (isUpdateLoad)
            {
                if (viewModel.Dispatcher.CheckAccess())
                    JobFinally();
                else
                    viewModel.Dispatcher.Invoke(JobFinally);
            }
        }
    }

    //public static async Task RunActionAsync(TgPageViewModelBase viewModel, Action action, bool isUpdateLoad)
    //{
    //    await Task.Delay(1).ConfigureAwait(false);

    //    void Job()
    //    {
    //        if (isUpdateLoad)
    //            viewModel.IsLoad = true;
    //        TgClientVm.Exception.Clear();
    //        action();
    //    }

    //    void JobFinally()
    //    {
    //        viewModel.IsLoad = false;
    //    }

    //    try
    //    {
    //        if (viewModel.Dispatcher.CheckAccess())
    //        {
    //            Job();
    //        }
    //        else
    //        {
    //            viewModel.Dispatcher.Invoke(() =>
    //            {
    //                Job();
    //            });
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        if (viewModel.Dispatcher.CheckAccess())
    //            TgClientVm.Exception.Set(ex);
    //        else
    //            viewModel.Dispatcher.Invoke(() => { TgClientVm.Exception.Set(ex); });
    //    }
    //    finally
    //    {
    //        if (isUpdateLoad)
    //        {
    //            if (viewModel.Dispatcher.CheckAccess())
    //                JobFinally();
    //            else
    //                viewModel.Dispatcher.Invoke(() =>
    //                {
    //                    JobFinally();
    //                });
    //        }
    //    }
    //}

    //public static async Task RunAction2Async(TgPageViewModelBase viewModel, Action action, bool isUpdateLoad)
    //{
    //    await Task.Delay(1).ConfigureAwait(false);

    //    async Task Job()
    //    {
    //        await Task.Delay(1).ConfigureAwait(false);
    //        if (isUpdateLoad)
    //            viewModel.IsLoad = true;
    //        TgClientVm.Exception.Clear();
    //        action();
    //    }

    //    async Task JobFinally()
    //    {
    //        await Task.Delay(1).ConfigureAwait(false);
    //        viewModel.IsLoad = false;
    //    }

    //    try
    //    {
    //        if (viewModel.Dispatcher.CheckAccess())
    //        {
    //            Job();
    //        }
    //        else
    //        {
    //            viewModel.Dispatcher.InvokeAsync(async () =>
    //            {
    //                await Job();
    //            });
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        if (viewModel.Dispatcher.CheckAccess())
    //            TgClientVm.Exception.Set(ex);
    //        else
    //            viewModel.Dispatcher.InvokeAsync(async () => { TgClientVm.Exception.Set(ex); });
    //    }
    //    finally
    //    {
    //        if (isUpdateLoad)
    //        {
    //            if (viewModel.Dispatcher.CheckAccess())
    //                JobFinally();
    //            else
    //                viewModel.Dispatcher.InvokeAsync(async () =>
    //                {
    //                    await JobFinally();
    //                });
    //        }
    //    }
    //}

    public static async Task RunFuncAsync(TgPageViewModelBase viewModel, Func<Task> action, bool isUpdateLoad)
    {
        await Task.Delay(1).ConfigureAwait(false);

        async Task Job()
        {
            if (isUpdateLoad)
                viewModel.IsLoad = true;
            TgClientVm.Exception.Clear();
            await action();
        }

        void JobFinally()
        {
            viewModel.IsLoad = false;
        }

        try
        {
            if (viewModel.Dispatcher.CheckAccess())
            {
                await Job();
            }
            else
            {
                await viewModel.Dispatcher.InvokeAsync(async () =>
                {
                    await Job();
                });
            }
        }
        catch (Exception ex)
        {
            if (viewModel.Dispatcher.CheckAccess())
                TgClientVm.Exception.Set(ex);
            else
                await viewModel.Dispatcher.InvokeAsync(() => TgClientVm.Exception.Set(ex));
        }
        finally
        {
            if (isUpdateLoad)
            {
                if (viewModel.Dispatcher.CheckAccess())
                    JobFinally();
                else
                    await viewModel.Dispatcher.InvokeAsync(JobFinally);
            }
        }
    }

    #endregion
}