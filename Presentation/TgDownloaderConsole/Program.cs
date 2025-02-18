// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

// DI
var containerBuilder = new ContainerBuilder();
containerBuilder.RegisterType<TgEfConsoleContext>().As<ITgEfContext>();
containerBuilder.RegisterType<TgConnectClientConsole>().As<ITgConnectClient>();
TgGlobalTools.Container = containerBuilder.Build();

// TgGlobalTools
var scope = TgGlobalTools.Container.BeginLifetimeScope();
TgGlobalTools.ConnectClient = scope.Resolve<ITgConnectClient>();

TgAppSettingsHelper.Instance.SetVersion(Assembly.GetExecutingAssembly());
var menu = new TgMenuHelper();

// Velopack installer update
await menu.VelopackUpdateAsync();

var tgLocale = TgLocaleHelper.Instance;
var tgLog = TgLogHelper.Instance;
var tgDownloadSettings = new TgDownloadSettingsViewModel();

// Create and update storage
tgLog.WriteLine("EF Core init ...");
await TgEfUtils.CreateAndUpdateDbAsync();
tgLog.WriteLine("EF Core init success");

// Menu
tgLog.WriteLine("Menu init ...");
TgGlobalTools.SetAppType(TgEnumAppType.Console);
tgLog.WriteLine("Menu init success");

// TG Connection
if (File.Exists(TgFileUtils.FileTgSession))
	await menu.ClientConnectAsync(tgDownloadSettings, isSilent: true);

do
{
	try
	{
		await menu.ShowTableMainAsync(tgDownloadSettings);
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
			.Title($"  {tgLocale.MenuSwitchNumber}")
			.PageSize(Console.WindowHeight - 17)
			.MoreChoicesText(tgLocale.MoveUpDown)
			.AddChoices(
				tgLocale.MenuMainExit, tgLocale.MenuMainApp, tgLocale.MenuMainConnection, tgLocale.MenuMainStorage, 
				tgLocale.MenuMainFilters, tgLocale.MenuMainDownload, tgLocale.MenuMainAdvanced, tgLocale.MenuMainUpdate));
		if (prompt.Equals(tgLocale.MenuMainExit))
			menu.Value = TgEnumMenuMain.Exit;
		if (prompt.Equals(tgLocale.MenuMainApp))
		{
			menu.Value = TgEnumMenuMain.AppSettings;
			await menu.SetupAppSettingsAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainConnection))
		{
			menu.Value = TgEnumMenuMain.Connection;
			await menu.SetupClientAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainStorage))
		{
			menu.Value = TgEnumMenuMain.Storage;
			await menu.SetupStorageAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainFilters))
		{
			menu.Value = TgEnumMenuMain.Filters;
			await menu.SetupFiltersAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainDownload))
		{
			menu.Value = TgEnumMenuMain.Download;
			await menu.SetupDownloadAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainAdvanced))
		{
			menu.Value = TgEnumMenuMain.Advanced;
			await menu.SetupAdvancedAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainUpdate))
		{
			menu.Value = TgEnumMenuMain.Update;
			await menu.VelopackUpdateAsync();
			tgLog.WriteLine(tgLocale.TypeAnyKeyForReturn);
			Console.ReadKey();
		}
	}
	catch (Exception ex)
	{
		tgLog.MarkupLine($"{tgLocale.StatusException}: " + tgLog.GetMarkupString(ex.Message));
		if (ex.InnerException is not null)
			tgLog.MarkupLine($"{tgLocale.StatusInnerException}: " + tgLog.GetMarkupString(ex.InnerException.Message));
		tgLog.WriteLine(tgLocale.TypeAnyKeyForReturn);
		Console.ReadKey();
	}
} while (menu.Value is not TgEnumMenuMain.Exit);
