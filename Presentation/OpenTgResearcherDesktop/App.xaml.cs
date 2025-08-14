// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
		if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
		{
			throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
		}
		return service;
	}

	public static WindowEx MainWindow { get; private set; } = default!;

    public static UIElement? AppTitlebar { get; set; }
    internal static ITgBusinessLogicManager BusinessLogicManager { get; private set; } = default!;
    private static ILifetimeScope Scope { get; set; } = default!;

    public App()
	{
		InitializeComponent();

        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Desktop);
        
		// Logging to the application directory
        TgLogUtils.Create(TgEnumAppType.Desktop, isAppStart: true);

        // Create ServiceCollection for EF Core Pooling
        var services = new ServiceCollection();
        services.AddDbContextPool<TgEfDesktopContext>(options =>
        {
            var context = new TgEfDesktopContext();
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
        containerBuilder.Register(c => serviceProvider.GetRequiredService<TgEfDesktopContext>())
            .As<ITgEfContext>()
            .InstancePerLifetimeScope();
        // Registering repositories
        containerBuilder.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        containerBuilder.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
        containerBuilder.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        containerBuilder.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        containerBuilder.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        containerBuilder.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        containerBuilder.RegisterType<TgEfMessageRelationRepository>().As<ITgEfMessageRelationRepository>();
        containerBuilder.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        containerBuilder.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        containerBuilder.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        containerBuilder.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Registering services
        containerBuilder.RegisterType<TgEfDesktopContext>().As<ITgEfContext>();
        containerBuilder.RegisterType<TgStorageManager>().As<ITgStorageManager>();
        containerBuilder.RegisterType<TgConnectClientDesktop>().As<ITgConnectClientDesktop>();
        containerBuilder.RegisterType<TgLicenseService>().As<ITgLicenseService>();
        containerBuilder.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>();
        // Building the container
        TgGlobalTools.Container = containerBuilder.Build();

        // Host
        Host = Microsoft.Extensions.Hosting.Host
			.CreateDefaultBuilder()
			.UseContentRoot(AppContext.BaseDirectory)
			.ConfigureServices((context, services) =>
			{
				// Default Activation Handler
				services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
				// Other Activation Handlers
				services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();
				// Services
				services.AddSingleton<IAppNotificationService, AppNotificationService>();
				services.AddSingleton<ITgSettingsService, TgSettingsService>();
				services.AddTransient<IWebViewService, WebViewService>();
				services.AddTransient<INavigationViewService, NavigationViewService>();
				services.AddSingleton<IActivationService, ActivationService>();
				services.AddSingleton<IPageService, PageService>();
				services.AddSingleton<INavigationService, NavigationService>();
				// Core Services
				services.AddSingleton<ISampleDataService, SampleDataService>();
				services.AddSingleton<IFileService, FileService>();
				// Views and ViewModels
				services.AddTransient<ContentGridDetailPage>();
				services.AddTransient<ContentGridDetailViewModel>();
				services.AddTransient<ContentGridPage>();
				services.AddTransient<ContentGridViewModel>();
				services.AddTransient<DataGridPage>();
				services.AddTransient<DataGridViewModel>();
				services.AddTransient<ListDetailsPage>();
				services.AddTransient<ListDetailsViewModel>();
				services.AddTransient<ShellPage>();
				services.AddTransient<ShellViewModel>();
				services.AddTransient<TgBotConnectionPage>();
				services.AddTransient<TgBotConnectionViewModel>();
				services.AddTransient<TgChatDetailsContentPage>();
				services.AddTransient<TgChatDetailsContentViewModel>();
				services.AddTransient<TgChatDetailsInfoPage>();
				services.AddTransient<TgChatDetailsInfoViewModel>();
				services.AddTransient<TgChatPage>();
				services.AddTransient<TgChatDetailsParticipantsPage>();
				services.AddTransient<TgChatDetailsParticipantsViewModel>();
				services.AddTransient<TgChatDetailsStatisticsPage>();
				services.AddTransient<TgChatDetailsStatisticsViewModel>();
				services.AddTransient<TgChatViewModel>();
				services.AddTransient<TgChatsPage>();
				services.AddTransient<TgChatsViewModel>();
				services.AddTransient<TgClientConnectionPage>();
				services.AddTransient<TgClientConnectionViewModel>();
				services.AddTransient<TgFiltersPage>();
				services.AddTransient<TgFiltersViewModel>();
				services.AddTransient<TgLicensePage>();
				services.AddTransient<TgLicenseViewModel>();
				services.AddTransient<TgLoadDataPage>();
				services.AddTransient<TgLoadDataViewModel>();
				services.AddTransient<TgLogsPage>();
				services.AddTransient<TgLogsViewModel>();
				services.AddTransient<TgMainPage>();
				services.AddTransient<TgMainViewModel>();
				services.AddTransient<TgProxiesPage>();
				services.AddTransient<TgProxiesViewModel>();
				services.AddTransient<TgSettingsPage>();
				services.AddTransient<TgSettingsViewModel>();
				services.AddTransient<TgSplashScreenPage>();
				services.AddTransient<TgSplashScreenViewModel>();
				services.AddTransient<TgStoragePage>();
				services.AddTransient<TgStorageViewModel>();
				services.AddTransient<TgStoriesPage>();
				services.AddTransient<TgStoriesViewModel>();
				services.AddTransient<TgUpdatePage>();
				services.AddTransient<TgUpdateViewModel>();
				services.AddTransient<TgUserDetailsPage>();
				services.AddTransient<TgUserDetailsViewModel>();
				services.AddTransient<TgUsersPage>();
				services.AddTransient<TgUsersViewModel>();
				services.AddTransient<WebViewPage>();
				services.AddTransient<WebViewViewModel>();
				// Configuration
				services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
			})
			.Build();
		
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
