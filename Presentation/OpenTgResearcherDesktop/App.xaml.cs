// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace OpenTgResearcherDesktop;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    #region Public and private fields, properties, constructor

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)!.Host.Services.GetRequiredService(typeof(T)) is not T service)
        {
            throw new InvalidOperationException($"{typeof(T)} is not registered in DI container or has incorrect type.");
        }
        return service;
    }

    public static object GetService(Type type)
    {
        var service = (Current as App)!.Host.Services.GetRequiredService(type);
        if (!type.IsInstanceOfType(service))
        {
            throw new InvalidOperationException($"{type} is not registered in DI container or has incorrect type.");
        }
        return service;
    }

    public static WindowEx MainWindow { get; private set; } = default!;

    public static UIElement? AppTitlebar { get; set; }
    internal static ITgBusinessLogicManager BusinessLogicManager { get; private set; } = default!;
    private static ILifetimeScope Scope { get; set; } = default!;
    public static TgViewModelLocator? Locator { get; set; } = default!;

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

                // Temporary provider for accessing SettingsService
                var tempProvider = services.BuildServiceProvider();
                var settingsService = tempProvider.GetRequiredService<ITgSettingsService>();
                var storagePath = settingsService.AppStorage;

                // Register the DbContext with SQLite
                services.AddDbContextPool<TgEfDesktopContext>(options =>
                {
                    options.UseSqlite($"Data Source={storagePath}");
#if DEBUG
                    options
                        .LogTo(message => Debug.WriteLine($"{TgGlobalTools.AppType}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging();
#endif
                    options
                        .EnableThreadSafetyChecks()
                        .UseLoggerFactory(new LoggerFactory());
                }, poolSize: 128);

                // Default Activation Handler
                services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
                // Other Activation Handlers
                services.AddSingleton<IActivationHandler, AppNotificationActivationHandler>();
                // Services
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<IWebViewService, WebViewService>();
                services.AddSingleton<INavigationViewService, NavigationViewService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<ISampleDataService, SampleDataService>();
                // Views and ViewModels
                services.AddSingleton<ContentGridDetailPage>();
                services.AddSingleton<ContentGridDetailViewModel>();
                services.AddSingleton<ContentGridPage>();
                services.AddSingleton<ContentGridViewModel>();
                services.AddSingleton<DataGridPage>();
                services.AddSingleton<DataGridViewModel>();
                services.AddSingleton<ListDetailsPage>();
                services.AddSingleton<ListDetailsViewModel>();
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<TgBotConnectionPage>();
                services.AddSingleton<TgBotConnectionViewModel>();
                services.AddSingleton<TgChatDetailsContentPage>();
                services.AddSingleton<TgChatDetailsContentViewModel>();
                services.AddSingleton<TgChatDetailsInfoPage>();
                services.AddSingleton<TgChatDetailsInfoViewModel>();
                services.AddSingleton<TgChatPage>();
                services.AddSingleton<TgChatDetailsParticipantsPage>();
                services.AddSingleton<TgChatDetailsParticipantsViewModel>();
                services.AddSingleton<TgChatDetailsStatisticsPage>();
                services.AddSingleton<TgChatDetailsStatisticsViewModel>();
                services.AddSingleton<TgChatViewModel>();
                services.AddSingleton<TgChatsPage>();
                services.AddSingleton<TgChatsViewModel>();
                services.AddSingleton<TgClientConnectionPage>();
                services.AddSingleton<TgClientConnectionViewModel>();
                services.AddSingleton<TgFiltersPage>();
                services.AddSingleton<TgFiltersViewModel>();
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
                services.AddSingleton<TgStoragePage>();
                services.AddSingleton<TgStorageViewModel>();
                services.AddSingleton<TgStoriesPage>();
                services.AddSingleton<TgStoriesViewModel>();
                services.AddSingleton<TgUpdatePage>();
                services.AddSingleton<TgUpdateViewModel>();
                services.AddSingleton<TgUserDetailsPage>();
                services.AddSingleton<TgUserDetailsViewModel>();
                services.AddSingleton<TgUsersPage>();
                services.AddSingleton<TgUsersViewModel>();
                services.AddSingleton<WebViewPage>();
                services.AddSingleton<WebViewViewModel>();
                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            })
            // Autofac registrations
            .ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule<TgAutofacRepositoryModule>();
                containerBuilder.RegisterModule<TgAutofacServiceModule>();
                // Registering the EF context via Host.Services
                containerBuilder.Register(c =>
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

    #region Public and private methods

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
            // ViewModel localor
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
