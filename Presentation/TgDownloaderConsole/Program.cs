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

// Helpers
var tgLocale = TgLocaleHelper.Instance;
var tgLog = TgLogHelper.Instance;
var tgDownloadSettings = new TgDownloadSettingsViewModel();
var tgAppSettings = TgAppSettingsHelper.Instance;
tgAppSettings.SetVersion(Assembly.GetExecutingAssembly());
var tgMenu = new TgMenuHelper();
var tgLicenseManager = TgLicenseManagerHelper.Instance;

// Console loading
Console.OutputEncoding = Encoding.UTF8;
Console.Title = TgConstants.AppTitleConsoleShort;
tgLog.SetMarkupLine(AnsiConsole.WriteLine);
tgLog.SetMarkupLineStamp(AnsiConsole.MarkupLine);
tgAppSettings.SetVersion(Assembly.GetExecutingAssembly());
tgLog.WriteLine($"{TgConstants.AppTitleConsole} {tgAppSettings.AppVersion}");

// Velopack installer update
//await menu.VelopackUpdateAsync(isWait: false, isRelease: true);
VelopackApp.Build()
#if WINDOWS
		.WithBeforeUninstallFastCallback((v) => {
			// delete / clean up some files before uninstallation
			tgLog.WriteLine($"Uninstalling the {TgConstants.AppTitleConsole}!");
		})
#endif
	.WithFirstRun((v) => {
		tgLog.WriteLine($"Thanks for installing the {TgConstants.AppTitleConsole}!");
	})
	.Run();

// Create and update storage
tgLog.WriteLine("Storage loading...");
await Task.Delay(250);
await TgEfUtils.CreateAndUpdateDbAsync();
tgLog.WriteLine("Storage loading complete");

// Menu
tgLog.WriteLine("Menu loading ...");
await Task.Delay(250);
TgGlobalTools.SetAppType(TgEnumAppType.Console);
tgLog.WriteLine("Menu loading complete");
await tgMenu.SetStorageVersionAsync();

// TG Connection
if (File.Exists(TgFileUtils.FileTgSession))
{
	tgLog.WriteLine("Connection loading ...");
	await tgMenu.ConnectClientAsync(tgDownloadSettings, isSilent: true);
	tgLog.WriteLine("Connection loading complete");
}

// License loading
tgLog.WriteLine("License loading ...");
tgLicenseManager.ActivateDefaultLicense();
await tgMenu.LicenseCheckAsync(tgDownloadSettings, isSilent: true);
tgLog.WriteLine("License loading complete");

do
{
	try
	{
		await tgMenu.ShowTableMainAsync(tgDownloadSettings);
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
			.Title($"  {tgLocale.MenuSwitchNumber}")
			.PageSize(Console.WindowHeight - 17)
			.MoreChoicesText(tgLocale.MoveUpDown)
			.AddChoices(
				tgLocale.MenuMainExit, tgLocale.MenuMainApp, tgLocale.MenuMainConnection, tgLocale.MenuMainStorage,
				tgLocale.MenuMainFilters, tgLocale.MenuMainDownload, tgLocale.MenuMainAdvanced, tgLocale.MenuMainUpdate, tgLocale.MenuMainLicense));
		if (prompt.Equals(tgLocale.MenuMainExit))
			tgMenu.Value = TgEnumMenuMain.Exit;
		if (prompt.Equals(tgLocale.MenuMainApp))
		{
			tgMenu.Value = TgEnumMenuMain.AppSettings;
			await tgMenu.SetupAppSettingsAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainConnection))
		{
			tgMenu.Value = TgEnumMenuMain.Connection;
			await tgMenu.SetupConnectionAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainStorage))
		{
			tgMenu.Value = TgEnumMenuMain.Storage;
			await tgMenu.SetupStorageAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainFilters))
		{
			tgMenu.Value = TgEnumMenuMain.Filters;
			await tgMenu.SetupFiltersAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainDownload))
		{
			tgMenu.Value = TgEnumMenuMain.Download;
			await tgMenu.SetupDownloadAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainAdvanced))
		{
			tgMenu.Value = TgEnumMenuMain.Advanced;
			await tgMenu.SetupAdvancedAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainUpdate))
		{
			tgMenu.Value = TgEnumMenuMain.Update;
			await tgMenu.SetupUpdateAsync(tgDownloadSettings);
		}
		if (prompt.Equals(tgLocale.MenuMainLicense))
		{
			tgMenu.Value = TgEnumMenuMain.License;
			await tgMenu.SetupLicenseAsync(tgDownloadSettings);
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
} while (tgMenu.Value is not TgEnumMenuMain.Exit);
