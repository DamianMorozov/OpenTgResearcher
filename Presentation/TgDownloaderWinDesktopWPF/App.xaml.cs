// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Autofac;
using TgInfrastructure.Enums;
using TgStorage.Connect;
using TgStorage.Domain;
using TgStorage.Domain.Contacts;

namespace TgDownloaderWinDesktopWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
	// The.NET Generic Host provides dependency injection, configuration, logging, and other services.
	// https://docs.microsoft.com/dotnet/core/extensions/generic-host
	// https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
	// https://docs.microsoft.com/dotnet/core/extensions/configuration
	// https://docs.microsoft.com/dotnet/core/extensions/logging
	private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
		.CreateDefaultBuilder()
		.ConfigureAppConfiguration(configurationBinder =>
        {
            string? path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
			if (!string.IsNullOrEmpty(path))
                configurationBinder.SetBasePath(path);
        })
		.ConfigureServices((context, services) =>
		{
			// App Host
			services.AddHostedService<ApplicationHostService>();
			// Register TgEfContext as the DbContext for EF Core
			//services.AddDbContext<TgEfContext>(options => options
			//	.UseSqlite(b => b.MigrationsAssembly(nameof(TgDownloaderWinDesktopWPF))));
			services.AddDbContextFactory<TgEfDesktopContext>();
			// Page resolver service
			services.AddSingleton<IPageService, PageService>();
			// Theme manipulation
			services.AddSingleton<IThemeService, ThemeService>();
			// TaskBar manipulation
			services.AddSingleton<ITaskBarService, TaskBarService>();
			// Service containing navigation, same as INavigationWindow... but without window
			services.AddSingleton<INavigationService, NavigationService>();
			// Main window with navigation
			services.AddScoped<INavigationWindow, MainWindow>();
			services.AddScoped<TgMainWindowViewModel>();
			// Core Services
			services.AddTransient<ITgEfContext, TgEfDesktopContext>();
			// Views and ViewModels
			services.AddTransient<TgDashboardPage>();
			services.AddTransient<TgDashboardViewModel>();
			services.AddTransient<TgSettingsPage>();
			services.AddTransient<TgSettingsViewModel>();
			services.AddTransient<TgSourcesPage>();
			services.AddTransient<TgSourcesViewModel>();
			services.AddTransient<TgClientPage>();
			services.AddTransient<TgClientViewModel>();
			services.AddTransient<TgProxiesPage>();
			services.AddTransient<TgProxiesViewModel>();
            services.AddTransient<TgItemSourcePage>();
            services.AddTransient<TgItemSourceViewModel>();
            services.AddTransient<TgItemProxyPage>();
            services.AddTransient<TgItemProxyViewModel>();
            services.AddTransient<TgDownloadsPage>();
			// Configuration
			services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
		}).Build();

	/// <summary>
	/// Gets registered service.
	/// </summary>
	/// <typeparam name="T">Type of the service to get.</typeparam>
	/// <returns>Instance of the service or <see langword="null"/>.</returns>
	public static T GetService<T>() where T : class, new() => 
        Host.Services.GetService(typeof(T)) as T ?? new T();

    /// <summary>
	/// Occurs when the application is loading.
	/// </summary>
	private async void OnStartup(object sender, StartupEventArgs e)
	{
		// DI
		var containerBuilder = new ContainerBuilder();
		containerBuilder.RegisterType<TgEfDesktopContext>().As<ITgEfContext>();
		containerBuilder.RegisterType<TgConnectClientDesktop>().As<ITgConnectClient>();
		TgGlobalTools.Container = containerBuilder.Build();
		// TgGlobalTools
		var scope = TgGlobalTools.Container.BeginLifetimeScope();
		TgGlobalTools.ConnectClient = scope.Resolve<ITgConnectClient>();
		// Create and update storage
		await TgEfUtils.CreateAndUpdateDbAsync();

		TgGlobalTools.SetAppType(TgEnumAppType.Desktop);
		
		await Host.StartAsync();
		TgDesktopUtils.SetupClient();
	}

	/// <summary>
	/// Occurs when the application is closing.
	/// </summary>
	private void OnExit(object sender, ExitEventArgs e)
	{
		var task = Host.StopAsync();
		task.Wait();
		Host.Dispose();
	}

	/// <summary>
	/// Occurs when an exception is thrown by an application but not handled.
	/// </summary>
	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		// For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
	}
}