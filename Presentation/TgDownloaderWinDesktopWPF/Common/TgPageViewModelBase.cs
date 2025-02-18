// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.Common;

/// <summary>
/// Base class for TgViewModel.
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgPageViewModelBase : TgViewModelBase
{
    #region Public and private fields, properties, constructor

	protected bool IsInitialized { get; set; }
	public bool IsFileSession { get; set; }
    public string StateConnectDt { get; set; } = string.Empty;
    public string StateConnectMsg { get; set; } = string.Empty;
    public string StateExceptionDt { get; set; } = string.Empty;
    public string StateExceptionMsg { get; set; } = string.Empty;
    public string StateProxyDt { get; set; } = string.Empty;
    public string StateProxyMsg { get; set; } = string.Empty;
    public string StateSourceDt { get; set; } = string.Empty;
    public string StateSourceMsg { get; set; } = string.Empty;
    public TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
    public TgExceptionViewModel Exception { get; set; } = new();
    public Dispatcher Dispatcher { get; set; } = Dispatcher.CurrentDispatcher;

    #endregion

    #region Public and private methods

    public override string ToString() => $"{TgDesktopUtils.TgLocale}";

    protected virtual void InitializeViewModel()
    {
        IsInitialized = true;
    }

    protected virtual async Task InitializeViewModelAsync()
    {
        await Task.Delay(1);
        IsInitialized = true;
    }

    /// <summary>
    /// Update state client message.
    /// </summary>
    /// <param name="message"></param>
    public virtual async Task UpdateStateConnectAsync(string message)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
	        await Task.Delay(1);
			StateConnectDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
            StateConnectMsg = message;
        }, false);
    }

    /// <summary>
    /// Update state client message.
    /// </summary>
    /// <param name="message"></param>
    public virtual async Task UpdateStateProxyAsync(string message)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
	        await Task.Delay(1);
			StateProxyDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
            StateProxyMsg = message;
        }, false);
    }

    /// <summary>
    /// Update exception message.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <param name="memberName"></param>
    /// <param name="message"></param>
    public virtual async Task UpdateStateExceptionAsync(string filePath, int lineNumber, string memberName, string message)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
	        await Task.Delay(1);
			StateExceptionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
            StateExceptionMsg = $"Line {lineNumber} | Member {memberName} | {message}";
        }, false);
    }

    /// <summary>
    /// Update exception message.
    /// </summary>
    /// <param name="message"></param>
    public virtual async Task UpdateStateExceptionShortAsync(string message)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
	        await Task.Delay(1);
			StateExceptionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
            StateExceptionMsg = message;
        }, false);
    }

    /// <summary>
    /// Update state source message.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="messageId"></param>
    /// <param name="message"></param>
    public virtual async Task UpdateStateSourceAsync(long sourceId, int messageId, int count, string message)
    {
		await TgDesktopUtils.RunFuncAsync(this, async () =>
		{
            if (sourceId == 0 && messageId == 0)
            {
                await UpdateStateMessageAsync(message);
                return;
            }
            else
            {
				StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
				StateSourceMsg = $"{sourceId} | {messageId} | {message}";
			}
		}, false);
    }

    /// <summary>
    /// Update state message.
    /// </summary>
    /// <param name="message"></param>
    public virtual async Task UpdateStateMessageAsync(string message)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
	        await Task.Delay(1);
			StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
            StateSourceMsg = message;
        }, false);
    }

    public async Task<bool> CheckClientReadyAsync()
    {
		return await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync();
    }

    #endregion
}