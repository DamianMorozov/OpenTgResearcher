//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//using TgStorage;

//namespace TgAssertCoreTests.Helpers;

//public class TgDataHelper
//{
//	#region Design pattern "Lazy Singleton"

//	private static TgDataHelper _instance = null!;
//	public static TgDataHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

//	#endregion

//	#region Fields, properties, constructor

//	public TgXpoContext ContextManager => TgXpoContext.Instance;
//	public TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;

//	public TgDataHelper()
//	{
//		TgAppSettings.LoadXmlSettings();
//		ContextManager.CreateOrConnectDb(false);
//	}

//	#endregion
//}