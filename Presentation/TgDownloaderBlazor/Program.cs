// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

// DI
using TgStorage.Contracts;

var containerBuilder = new ContainerBuilder();
containerBuilder.RegisterType<TgEfBlazorContext>().As<ITgEfContext>();
containerBuilder.RegisterType<TgConnectClientBlazor>().As<ITgConnectClient>();
TgGlobalTools.Container = containerBuilder.Build();

// TgGlobalTools
var scope = TgGlobalTools.Container.BeginLifetimeScope();
TgGlobalTools.ConnectClient = scope.Resolve<ITgConnectClient>();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Radzen
// https://blazor.radzen.com/get-started
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<TgJsService>();
// DI
builder.Services.AddHttpClient();
builder.Services.AddTransient<ITgEfContext, TgEfBlazorContext>();

// Register TgEfContext as the DbContext for EF Core
//builder.Services.AddDbContextFactory<TgEfBlazorContext>(options => options
//	.UseSqlite(b => b.MigrationsAssembly(nameof(TgDownloaderBlazor))));
// Create and update storage
await TgEfUtils.CreateAndUpdateDbAsync();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
