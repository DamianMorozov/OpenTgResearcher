namespace OpenTgResearcherConsole;

public static class Program
{
    public static async Task Main()
    {
        WaitForMakeScreenRecord();

        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Console);

        // Registering .NET DI services
        var services = new ServiceCollection();

        // Get storage path
        var context = new TgEfConsoleContext();
        var storagePath = context.GetDataSource();

        // Register the DbContext with SQLite
        services.AddDbContextPool<TgEfConsoleContext>(options =>
        {
            options.UseSqlite(storagePath);
            // Copy by TgEfConsoleContext.OnConfiguring
            options
#if DEBUG
                .LogTo(message => Debug.WriteLine($"{TgGlobalTools.AppType}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
#endif
                .EnableThreadSafetyChecks()
                .UseLoggerFactory(new LoggerFactory());

        }, poolSize: 128);

        // Register FusionCache
        services.AddFusionCache().WithDefaultEntryOptions(TgCacheUtils.CacheOptionsChannelMessages);
        var serviceProvider = services.BuildServiceProvider();

        // Get configured FusionCache instance from MS DI
        var fusionCache = serviceProvider.GetRequiredService<IFusionCache>();
        // Register the Autofac DI
        var cb = new ContainerBuilder();
        // Make FusionCache available for Autofac graph
        cb.RegisterInstance(fusionCache).As<IFusionCache>().SingleInstance();
        // Registering the EF context
        cb.Register(c =>
        {
            var context = new TgEfConsoleContext();
            // Create DbContextOptionsBuilder with SQLite connection
            var optionsBuilder = new DbContextOptionsBuilder<TgEfConsoleContext>();
            optionsBuilder.UseSqlite(context.GetDataSource());
            return new TgEfConsoleContext(optionsBuilder.Options);
        }).As<ITgEfContext>().InstancePerLifetimeScope();
        // Registering repositories
        cb.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        cb.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
        cb.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        cb.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        cb.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        cb.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        cb.RegisterType<TgEfMessageRelationRepository>().As<ITgEfMessageRelationRepository>();
        cb.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        cb.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        cb.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        cb.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Registering services
        cb.RegisterType<TgStorageService>().As<ITgStorageService>().SingleInstance();
        cb.RegisterType<TgFloodControlService>().As<ITgFloodControlService>().SingleInstance();
        cb.RegisterType<TgConnectClientConsole>().As<ITgConnectClientConsole>().SingleInstance();
        cb.RegisterType<TgLicenseService>().As<ITgLicenseService>().SingleInstance();
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().SingleInstance();
        cb.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>().SingleInstance();
        // We build a container and take IServiceProvider
        var container = cb.Build();
        TgGlobalTools.Container = container;

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
        TgLogUtils.Create(TgEnumAppType.Console, isAppStart: true);
        tgLog.WriteLine("  Loading logging   v");

        // Clear FusionCache on startup
        await tgMenu.BusinessLogicManager.Cache.ClearAllAsync();

        // Loading storage
        tgLog.WriteLine("  Loading storage ...");
        await Task.Delay(250);
        await tgMenu.BusinessLogicManager.StorageManager.CreateAndUpdateDbAsync();
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
        if (tgMenu.CheckMultipleInstances()) return;

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
                    tgLocale.MenuMainClientConnection,
                    tgLocale.MenuMainBotConnection,
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
                TgMenuHelper.CatchException(ex);
            }
        } while (tgMenu.Value is not TgEnumMenuMain.Exit);
    }

    /// <summary> Loading Velopack Installer </summary>
    private static void LoadingVelopackInstaller(TgLogHelper tgLog)
    {
        tgLog.WriteLine("  Loading installer ...");
        // Velopack installer update
        VelopackApp.Build()
            .OnFirstRun((v) =>
            {
                TgLogUtils.WriteLog($"  Thanks for installing the {TgConstants.OpenTgResearcherConsole}!");
                tgLog.WriteLine($"  Thanks for installing the {TgConstants.OpenTgResearcherConsole}!");
            })
            .Run();
        tgLog.WriteLine("  Loading installer   v");
    }

    /// <summary> Wait for make screen record </summary>
    private static void WaitForMakeScreenRecord(int seconds = 3)
    {
#if DEBUG
        for (int i = seconds; i > 0; i--)
        {
            Console.WriteLine("Wait for make screen record");
            Console.Write($"Seconds left: {i}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.Clear();
        }
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
#endif
    }
}
