// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop;

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
		if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
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
		// DI
		var containerBuilder = new ContainerBuilder();
		containerBuilder.RegisterType<TgEfDesktopContext>().As<ITgEfContext>();
		containerBuilder.RegisterType<TgConnectClientDesktop>().As<ITgConnectClient>();
		TgGlobalTools.Container = containerBuilder.Build();
		// TgGlobalTools
		using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		TgGlobalTools.ConnectClient = scope.Resolve<ITgConnectClient>();
		// Velopack
		VelopackApp.Build()
			//.WithBeforeUninstallFastCallback((v) =>
			//{
			//	delete / clean up some files before uninstallation
			//   UpdateLog += $"Uninstalling the {TgConstants.AppTitleConsole}!";
			//})
			//.WithFirstRun((v) =>
			//{
			//	UpdateLog += $"Thanks for installing the {TgConstants.AppTitleConsole}!";
			//})
			.Run();
		// License
		TgLicenseManagerHelper.Instance.ActivateLicense(string.Empty, TgResourceExtensions.GetLicenseFreeDescription(), 
			TgResourceExtensions.GetLicenseTestDescription(), TgResourceExtensions.GetLicensePaidDescription(), 
			TgResourceExtensions.GetLicensePremiumDescription());
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

		GetService<IAppNotificationService>().Initialize();
		UnhandledException += App_UnhandledException;
		// Logger
		var appFolder = GetService<ITgSettingsService>().AppFolder;
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.File(Path.Combine(appFolder, $"{TgFileUtils.LogsDirectory}/Log-.txt"), rollingInterval: RollingInterval.Day, shared: true)
			.CreateLogger();
	}

	~App()
	{
		Host.Dispose();
		Log.CloseAndFlush();
	}

	#endregion

	#region Public and private methods

	private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		// TODO: Log and handle exceptions as appropriate.
		// https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
		TgLogUtils.LogFatal(e.Exception, sender.ToString() ?? string.Empty);
		// Set a handled exception to prevent the application from terminating
		e.Handled = true;
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		base.OnLaunched(args);
		GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

		await MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
		{
			await GetService<IActivationService>().ActivateAsync(args);
			// Register TgEfContext as the DbContext for EF Core
			TgEfUtils.AppStorage = GetService<ITgSettingsService>().AppStorage;
			// Create and update storage
			await TgEfUtils.CreateAndUpdateDbAsync();
		}, "Application launched");
	}

	#endregion
}
