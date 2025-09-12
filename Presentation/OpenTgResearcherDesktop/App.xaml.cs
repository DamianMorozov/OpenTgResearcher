// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    #region Fields, properties, constructor

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    internal IHost Host { get; }

    internal static T GetService<T>() where T : class
    {
        if ((Current as App)!.Host.Services.GetRequiredService(typeof(T)) is not T service)
        {
            throw new InvalidOperationException($"{typeof(T)} is not registered in DI container or has incorrect type.");
        }
        return service;
    }

    internal static object GetService(Type type)
    {
        var service = (Current as App)!.Host.Services.GetRequiredService(type);
        if (!type.IsInstanceOfType(service))
        {
            throw new InvalidOperationException($"{type} is not registered in DI container or has incorrect type.");
        }
        return service;
    }

    internal static WindowEx MainWindow { get; private set; } = default!;

    internal static UIElement? AppTitleBar { get; private set; }
    internal static ITgBusinessLogicManager BusinessLogicManager { get; private set; } = default!;
    internal static ILifetimeScope Scope { get; private set; } = default!;
    internal static TgViewModelLocator? Locator { get; set; } = default!;

    public App()
    {
        InitializeComponent();

        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Desktop);

        // Logging to the application directory
        TgLogUtils.Create(TgEnumAppType.Desktop, isAppStart: true);

        // Host
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory()) // Connecting Autofac
            .ConfigureServices((context, services) =>
            {
                // Startup services
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<ITgSettingsService, TgSettingsService>();
                services.AddSingleton<IPageService, PageService>();

                // Get storage path
                var tempProvider = services.BuildServiceProvider();
                var settingsService = tempProvider.GetRequiredService<ITgSettingsService>();
                var storagePath = $"Data Source={settingsService.AppStorage}";

                // Register the DbContext with SQLite
                services.AddDbContextPool<TgEfDesktopContext>(options =>
                {
                    options.UseSqlite();
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
                // Default Activation Handler
                services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
                // Other Activation Handlers
                services.AddSingleton<IActivationHandler, AppNotificationActivationHandler>();
                // Services
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<IWebViewService, WebViewService>();
                services.AddSingleton<INavigationViewService, NavigationViewService>();
                services.AddSingleton<IActivationService, ActivationService>();
                // Views and ViewModels
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<TgChatDetailsContentPage>();
                services.AddSingleton<TgChatDetailsContentViewModel>();
                services.AddSingleton<TgChatDetailsInfoPage>();
                services.AddSingleton<TgChatDetailsInfoViewModel>();
                services.AddSingleton<TgChatDetailsParticipantsPage>();
                services.AddSingleton<TgChatDetailsParticipantsViewModel>();
                services.AddSingleton<TgChatDetailsStatisticsPage>();
                services.AddSingleton<TgChatDetailsStatisticsViewModel>();
                services.AddSingleton<TgChatPage>();
                services.AddSingleton<TgChatsPage>();
                services.AddSingleton<TgChatsViewModel>();
                services.AddSingleton<TgChatViewModel>();
                services.AddSingleton<TgClientConnectionPage>();
                services.AddSingleton<TgClientConnectionViewModel>();
                services.AddSingleton<TgFiltersPage>();
                services.AddSingleton<TgFiltersViewModel>();
                services.AddSingleton<TgHardwareControlPage>();
                services.AddSingleton<TgHardwareControlViewModel>();
                services.AddSingleton<TgLicensePage>();
                services.AddSingleton<TgLicenseViewModel>();
                services.AddSingleton<TgLoadDataPage>();
                services.AddSingleton<TgLoadDataViewModel>();
                services.AddSingleton<TgLogsPage>();
                services.AddSingleton<TgLogsViewModel>();
                services.AddSingleton<TgMainPage>();
                services.AddSingleton<TgMainViewModel>();
                services.AddSingleton<TgProxiesPage>();
                services.AddSingleton<TgProxiesViewModel>();
                services.AddSingleton<TgSettingsPage>();
                services.AddSingleton<TgSettingsViewModel>();
                services.AddSingleton<TgSplashScreenPage>();
                services.AddSingleton<TgSplashScreenViewModel>();
                services.AddSingleton<TgStorageAdvancedPage>();
                services.AddSingleton<TgStorageAdvancedViewModel>();
                services.AddSingleton<TgStorageConfigurationPage>();
                services.AddSingleton<TgStorageConfigurationViewModel>();
                services.AddSingleton<TgStoragePage>();
                services.AddSingleton<TgStorageTablesPage>();
                services.AddSingleton<TgStorageTablesViewModel>();
                services.AddSingleton<TgStorageViewModel>();
                services.AddSingleton<TgStoriesPage>();
                services.AddSingleton<TgStoriesViewModel>();
                services.AddSingleton<TgUpdatePage>();
                services.AddSingleton<TgUpdateViewModel>();
                services.AddSingleton<TgUserDetailsPage>();
                services.AddSingleton<TgUserDetailsViewModel>();
                services.AddSingleton<TgUsersPage>();
                services.AddSingleton<TgUsersViewModel>();
                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            })
            // Autofac registrations
            .ConfigureContainer<ContainerBuilder>(cb =>
            {
                cb.RegisterModule<TgAutofacRepositoryModule>();
                cb.RegisterModule<TgAutofacServiceModule>();
                // Registering the EF context via Host.Services
                cb.Register(c =>
                {
                    // Temporary provider for accessing SettingsService
                    var tempProvider = c.Resolve<IServiceProvider>();
                    var settingsService = tempProvider.GetRequiredService<ITgSettingsService>();
                    var storagePath = settingsService.AppStorage;
                    // Create DbContextOptionsBuilder with SQLite connection
                    var optionsBuilder = new DbContextOptionsBuilder<TgEfDesktopContext>();
                    optionsBuilder.UseSqlite($"Data Source={storagePath}");
                    return new TgEfDesktopContext(optionsBuilder.Options);
                }).As<ITgEfContext>().InstancePerLifetimeScope();
            })
            .Build();

        var desktopContext = Host.Services.GetRequiredService<TgEfDesktopContext>();

        // Set the Host as the current application host
        TgGlobalTools.Container = (IContainer)Host.Services.GetAutofacRoot();

        // Exceptions
        UnhandledException += App_UnhandledException;

        AppDomain.CurrentDomain.ProcessExit += (s, e) => OnProcessExit();
    }

    public void OnProcessExit()
    {
        Host.Dispose();
        TgLogUtils.CloseAndFlush();

        BusinessLogicManager.Dispose();
        Scope.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion

    #region Methods

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        TgLogUtils.WriteException(e.Exception);
        //GetService<IAppNotificationService>().Show(message);

        // Set a handled exception to prevent the application from terminating
        e.Handled = true;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            base.OnLaunched(args);

            // TG client loading
            Scope = TgGlobalTools.Container.BeginLifetimeScope();
            BusinessLogicManager = Scope.Resolve<ITgBusinessLogicManager>();
            // Create and update storage
            await BusinessLogicManager.StorageManager.CreateAndUpdateDbAsync();
            // Clear FusionCache on startup
            await BusinessLogicManager.Cache.ClearAllAsync();
            // ViewModel locator
            Locator = new();

            // MainWindow
            MainWindow ??= new MainWindow();
            if (MainWindow?.DispatcherQueue is not null)
            {
                var tcs = new TaskCompletionSource();
                await MainWindow.DispatcherQueue.EnqueueAsync(async () =>
                {
                    try
                    {
                        // Activate the app
                        await GetService<IActivationService>().ActivateAsync(args);
                        tcs.SetResult();
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                await tcs.Task;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    #endregion
}
