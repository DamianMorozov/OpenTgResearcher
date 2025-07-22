// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherConsole;

public static class Program
{
    public static async Task Main()
    {
        // Uncomment for make screen record
        //WaitForMakeScreenRecord(seconds: 5);

        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Console);

        // Create ServiceCollection for EF Core Pooling
        var services = new ServiceCollection();
        services.AddDbContextPool<TgEfConsoleContext>(options =>
        {
            var context = new TgEfConsoleContext();
            options.UseSqlite(context.GetStoragePath());
            // Copy by TgEfConsoleContext.OnConfiguring
            LoggerFactory factory = new();
            options
#if DEBUG
                .LogTo(message => Debug.WriteLine($"{TgGlobalTools.AppType}{nameof(context.ContextId)} {context.ContextId}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
#endif
                .EnableThreadSafetyChecks()
                .UseLoggerFactory(factory);

        }, poolSize: 128);
        var serviceProvider = services.BuildServiceProvider();

        // DI register
        var containerBuilder = new ContainerBuilder();
        // Register DbContext from ServiceProvider
        containerBuilder.Register(c => serviceProvider.GetRequiredService<TgEfConsoleContext>())
            .As<ITgEfContext>()
            .InstancePerLifetimeScope();
        // Registering repositories
        containerBuilder.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        containerBuilder.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
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
        using var tgMenu = new TgMenuHelper();

        // Loading Console
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = TgConstants.OTR;
        tgLog.SetMarkupLine(AnsiConsole.WriteLine);
        tgLog.SetMarkupLineStamp(AnsiConsole.MarkupLine);
        tgAppSettings.SetVersion(Assembly.GetExecutingAssembly());
        tgLog.WriteLine("");
        tgLog.WriteLine($"  {TgConstants.OpenTgResearcherConsole} {tgAppSettings.AppVersion}");
        tgLog.WriteLine("");

        // Loading Velopack Installer
        LoadingVelopackInstaller(tgLog);

        // Loading logging
        tgLog.WriteLine("  Loading logging ...");
        TgLogUtils.InitStartupLog(TgConstants.OpenTgResearcherConsole, isWebApp: false, isRewrite: true);
        tgLog.WriteLine("  Loading logging   v");

        // Loading storage
        tgLog.WriteLine("  Loading storage ...");
        await Task.Delay(250);
        await tgMenu.BusinessLogicManager.CreateAndUpdateDbAsync();
        await tgMenu.SetStorageVersionAsync();
        tgLog.WriteLine("  Loading storage   v");

        // Loading license
        tgLog.WriteLine("  Loading license ...");
        await tgMenu.BusinessLogicManager.LicenseService.LicenseActivateAsync();
        await tgMenu.LicenseCheckOnlineAsync(tgDownloadSettings, isSilent: true);
        tgLog.WriteLine("  Loading license   v");

        // Loading TG Connection
        //if (File.Exists(TgFileUtils.FileTgSession))
        //{
        //    tgLog.WriteLine("  Loading connection ...");
        //    await tgMenu.ConnectClientAsync(tgDownloadSettings, isSilent: true);
        //    tgLog.WriteLine("  Loading connection   v");
        //}

        // Check multiple instances
        if (tgMenu.CheckMultipleInstances())
            return;

        // Any key
        tgLog.WriteLine("");
        tgLog.TypeAnyKeyForReturn();

        do
        {
            try
            {
                await tgMenu.ShowTableMainAsync(tgDownloadSettings);
                var selectionPrompt = new SelectionPrompt<string>()
                    .Title($"  {tgLocale.MenuSwitchNumber}")
                    .PageSize(Console.WindowHeight - 17)
                    .MoreChoicesText(tgLocale.MoveUpDown);
                selectionPrompt.AddChoices(
                    tgLocale.MenuMainExit,
                    tgLocale.MenuMainApp,
                    tgLocale.MenuMainStorage,
                    tgLocale.MenuMainClientConnection
                );
                // Check paid license
                if (tgMenu.BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
                {
                    selectionPrompt.AddChoices(tgLocale.MenuMainBotConnection);
                }
                selectionPrompt.AddChoices(
                    tgLocale.MenuMainUpdate,
                    tgLocale.MenuMainLicense
                );

                var prompt = AnsiConsole.Prompt(selectionPrompt);
                if (prompt.Equals(tgLocale.MenuMainExit))
                    tgMenu.Value = TgEnumMenuMain.Exit;
                if (prompt.Equals(tgLocale.MenuMainApp))
                {
                    tgMenu.Value = TgEnumMenuMain.AppSettings;
                    await tgMenu.SetupAppSettingsAsync(tgDownloadSettings);
                }
                if (prompt.Equals(tgLocale.MenuMainClientConnection))
                {
                    tgMenu.Value = TgEnumMenuMain.ClientConnection;
                    await tgMenu.SetupClientConAsync(tgDownloadSettings);
                }
                if (prompt.Equals(tgLocale.MenuMainBotConnection))
                {
                    tgMenu.Value = TgEnumMenuMain.BotConnection;
                    await tgMenu.SetupBotConAsync(tgDownloadSettings);
                }
                if (prompt.Equals(tgLocale.MenuMainStorage))
                {
                    tgMenu.Value = TgEnumMenuMain.Storage;
                    await tgMenu.SetupStorageAsync(tgDownloadSettings);
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
                tgMenu.CatchException(ex);
            }
        } while (tgMenu.Value is not TgEnumMenuMain.Exit);
    }

    /// <summary> Loading Velopack Installer </summary>
    private static void LoadingVelopackInstaller(TgLogHelper tgLog)
    {
        tgLog.WriteLine("  Loading installer ...");
        // Velopack installer update
        //await menu.VelopackUpdateAsync(isWait: false, isRelease: true);
        VelopackApp.Build()
#if WINDOWS
		.WithBeforeUninstallFastCallback((v) => {
			// delete / clean up some files before uninstallation
			tgLog.WriteLine($"  Uninstalling the {TgConstants.OpenTgResearcherConsole}!");
		})
#endif
            .OnFirstRun((v) =>
            {
                tgLog.WriteLine($"  Thanks for installing the {TgConstants.OpenTgResearcherConsole}!");
            })
            .Run();
        tgLog.WriteLine("  Loading installer   v");
    }

    /// <summary> Wait for make screen record </summary>
    private static void WaitForMakeScreenRecord(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            Console.WriteLine("Wait for make screen record");
            Console.Write($"Seconds left: {i}");
            Thread.Sleep(1_000);
            Console.Clear();
        }
        Thread.Sleep(3_000);
    }
}
