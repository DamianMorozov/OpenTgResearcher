﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherConsole;

public static class Program
{
    public static async Task Main()
	{
        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Console);
        
        // DI register
        var containerBuilder = new ContainerBuilder();
        // Registering repositories
        containerBuilder.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        containerBuilder.RegisterType<TgEfContactRepository>().As<ITgEfContactRepository>();
        containerBuilder.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        containerBuilder.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        containerBuilder.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        containerBuilder.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        containerBuilder.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        containerBuilder.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        containerBuilder.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        containerBuilder.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Registering services
        containerBuilder.RegisterType<TgStorageManager>().As<ITgStorageManager>();
        containerBuilder.RegisterType<TgEfConsoleContext>().As<ITgEfContext>();
        containerBuilder.RegisterType<TgConnectClientConsole>().As<ITgConnectClientConsole>();
        containerBuilder.RegisterType<TgLicenseService>().As<ITgLicenseService>();
        containerBuilder.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>();
        // Building the container
        TgGlobalTools.Container = containerBuilder.Build();

        // Helpers
        var tgLocale = TgLocaleHelper.Instance;
        var tgLog = TgLogHelper.Instance;
        var tgDownloadSettings = new TgDownloadSettingsViewModel();
        var tgAppSettings = TgAppSettingsHelper.Instance;
        tgAppSettings.SetVersion(Assembly.GetExecutingAssembly());
        var tgMenu = new TgMenuHelper();

        // Loading Console
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = TgConstants.OTR;
        tgLog.SetMarkupLine(AnsiConsole.WriteLine);
        tgLog.SetMarkupLineStamp(AnsiConsole.MarkupLine);
        tgAppSettings.SetVersion(Assembly.GetExecutingAssembly());
        tgLog.WriteLine($"{TgConstants.OpenTgResearcherConsole} {tgAppSettings.AppVersion}");

        // Loading Velopack Installer
        LoadingVelopackInstaller(tgLog);

        // Loading License
        tgLog.WriteLine("Loading license ...");
        tgMenu.BusinessLogicManager.LicenseService.ActivateDefaultLicense();
        await tgMenu.LicenseCheckAsync(tgDownloadSettings, isSilent: true);
        tgLog.WriteLine("Loading license - v");

        // Loading storage
        tgLog.WriteLine("Loading storage ...");
        await Task.Delay(250);
        await tgMenu.BusinessLogicManager.CreateAndUpdateDbAsync();
        await tgMenu.SetStorageVersionAsync();
        tgLog.WriteLine("Loading storage - v");

        // Loading TG Connection
        if (File.Exists(TgFileUtils.FileTgSession))
        {
            tgLog.WriteLine("Loading connection ...");
            await tgMenu.ConnectClientAsync(tgDownloadSettings, isSilent: true);
            tgLog.WriteLine("Loading connection - v");
        }

        // Any key
        tgLog.TypeAnyKeyForReturn();

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
                tgLog.TypeAnyKeyForReturn();
            }
        } while (tgMenu.Value is not TgEnumMenuMain.Exit);
    }

    /// <summary> Loading Velopack Installer </summary>
    private static void LoadingVelopackInstaller(TgLogHelper tgLog)
	{
        tgLog.WriteLine("Loading installer ...");
        // Velopack installer update
        //await menu.VelopackUpdateAsync(isWait: false, isRelease: true);
        VelopackApp.Build()
#if WINDOWS
		.WithBeforeUninstallFastCallback((v) => {
			// delete / clean up some files before uninstallation
			tgLog.WriteLine($"Uninstalling the {TgConstants.AppTitleConsole}!");
		})
#endif
            .OnFirstRun((v) => {
                tgLog.WriteLine($"Thanks for installing the {TgConstants.OpenTgResearcherConsole}!");
            })
            .Run();
        tgLog.WriteLine("Loading installer - v");
    }
}
