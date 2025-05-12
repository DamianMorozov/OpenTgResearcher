// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Utils;

/// <summary> Global tools </summary>
public static class TgGlobalTools
{
    #region Public and private fields, properties, constructor

    public static TgEnumAppType AppType { get; private set; }

    public static bool IsXmlReady => TgAppSettingsHelper.Instance.AppXml.IsExistsEfStorage;

    /// <summary> Autofac Container </summary>
	public static Autofac.IContainer Container = null!;
	/// <summary> Telegram client </summary>
	public static ITgConnectClient ConnectClient { get; set; } = null!;
	/// <summary> Limit count of download threads by free license </summary>
	public static int DownloadCountThreadsLimitFree => 10;
	/// <summary> Limit count of download threads by test license </summary>
	public static int DownloadCountThreadsLimitPaid => 100;
	/// <summary> Limit batch of saving messages </summary>
	public static int BatchMessagesLimit => 100;


	#endregion

	#region Public and private methods

	public static void SetAppType(TgEnumAppType appType)
    {
        AppType = appType;
    }

	#endregion

	#region Constants
	
	public const string HttpHeaderContentTypeJson = "application/json";
	public const string RouteChangeLog = "ChangeLog";
	public const string RouteController = "[controller]";
	public const string RouteCreated = "Created";
	public const string RouteGet = "Get";
	public const string RoutePost = "Post";
	public const string RouteRoot = "";
	
	#endregion
}