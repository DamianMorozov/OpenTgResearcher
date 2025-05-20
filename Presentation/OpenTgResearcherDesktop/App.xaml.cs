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

	public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

	public App()
	{
		InitializeComponent();

        // Logging to the application directory
        TgLogUtils.InitStartupLog();
        TgLogUtils.WriteToLog($"App started");

        // DI
        var containerBuilder = new ContainerBuilder();
		containerBuilder.RegisterType<TgEfDesktopContext>().As<ITgEfContext>();
		containerBuilder.RegisterType<TgConnectClientDesktop>().As<ITgConnectClientDesktop>();
		TgGlobalTools.Container = containerBuilder.Build();
		// Host
		Host = Microsoft.Extensions.Hosting.Host
			.CreateDefaultBuilder()
			.UseSerilog()
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
				services.AddSingleton<ITgLicenseService, TgLicenseService>();
				services.AddSingleton<IFileService, FileService>();
				services.AddTransient<ITgEfContext, TgEfDesktopContext>();
				// Views and ViewModels
				services.AddTransient<ShellViewModel>();
				services.AddTransient<TgSettingsViewModel>();
				services.AddTransient<TgSettingsPage>();
				services.AddTransient<DataGridViewModel>();
				services.AddTransient<DataGridPage>();
				services.AddTransient<ContentGridDetailViewModel>();
				services.AddTransient<ContentGridDetailPage>();
				services.AddTransient<ContentGridViewModel>();
				services.AddTransient<ContentGridPage>();
				services.AddTransient<ListDetailsViewModel>();
				services.AddTransient<ListDetailsPage>();
				services.AddTransient<WebViewViewModel>();
				services.AddTransient<WebViewPage>();
				services.AddTransient<TgMainViewModel>();
				services.AddTransient<TgMainPage>();
				services.AddTransient<ShellPage>();
				services.AddTransient<TgConnectViewModel>();
				services.AddTransient<TgConnectPage>();
				services.AddTransient<TgLoadDataViewModel>();
				services.AddTransient<TgLoadDataPage>();
				services.AddTransient<TgContactsViewModel>();
				services.AddTransient<TgContactsPage>();
				services.AddTransient<TgContactDetailsViewModel>();
				services.AddTransient<TgContactDetailsPage>();
				services.AddTransient<TgChatDetailsViewModel>();
				services.AddTransient<TgChatDetailsPage>();
				services.AddTransient<TgChatsViewModel>();
				services.AddTransient<TgChatsPage>();
				services.AddTransient<TgFiltersViewModel>();
				services.AddTransient<TgFiltersPage>();
				services.AddTransient<TgStoriesViewModel>();
				services.AddTransient<TgStoriesPage>();
				services.AddTransient<TgProxiesViewModel>();
				services.AddTransient<TgProxiesPage>();
				services.AddTransient<TgUpdateViewModel>();
				services.AddTransient<TgUpdatePage>();
				services.AddTransient<TgSplashScreenViewModel>();
				services.AddTransient<TgSplashScreenPage>();
				services.AddTransient<TgLicenseViewModel>();
				services.AddTransient<TgLicensePage>();
				services.AddTransient<TgLogsViewModel>();
				services.AddTransient<TgLogsPage>();
				// Logger
				services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
				// Configuration
				services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
			})
			.Build();
		
		// Exceptions
		UnhandledException += App_UnhandledException;
		
		// TG client loading
		using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		TgGlobalTools.ConnectClient = scope.Resolve<ITgConnectClientDesktop>();
	}

	~App()
	{
		Host.Dispose();
		Log.CloseAndFlush();
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

			await MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
			{
                // Activate the app
                await GetService<IActivationService>().ActivateAsync(args);
            }, "Application launched");
		}
		catch (Exception ex)
		{
            TgLogUtils.WriteException(ex);
		}
	}

	#endregion
}
