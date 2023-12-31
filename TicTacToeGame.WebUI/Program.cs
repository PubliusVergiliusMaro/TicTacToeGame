using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.Services.HubConnections;
using TicTacToeGame.Services.RoomServices;
using TicTacToeGame.WebUI.BackgroundServices;
using TicTacToeGame.WebUI.Components;
using TicTacToeGame.WebUI.Components.Account;
using TicTacToeGame.WebUI.Data;
using TicTacToeGame.WebUI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IdentityUserAccessor>();

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<Player>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<GamesHistoryRepository>(rep => new GamesHistoryRepository(connectionString));

builder.Services.AddSingleton<GameRepository>(rep => new GameRepository(connectionString));

builder.Services.AddSingleton<PlayerRepository>(rep => new PlayerRepository(connectionString));

builder.Services.AddSingleton<RoomRepository>(rep => new RoomRepository(connectionString));

builder.Services.AddScoped<GamesStatisticsService>();

builder.Services.AddSingleton<IEmailSender<Player>, IdentityNoOpEmailSender>();

builder.Services.AddSingleton<RoomManagerService>();

builder.Services.AddTransient<TemporaryRoomService>();

builder.Services.AddScoped<GameManager>();

builder.Services.AddTransient<GameInitializeService>();

builder.Services.AddScoped<CheckForWinnerManager>();

builder.Services.AddTransient<MakeMovesGameManager>();

builder.Services.AddTransient<PlayerDisconectingTrackingService>();

builder.Services.AddTransient<GameChatService>();

builder.Services.AddTransient<GameReconnectingService>();

builder.Services.AddTransient<GameSessionService>();

builder.Services.AddTransient<JoinRoomHubConnection>();

builder.Services.AddScoped<GameHubConnection>();

builder.Services.AddTransient<HostRoomHubConnection>();

builder.Services.AddTransient<GameCleaner>();

builder.Services.AddSingleton<GameBoardManager>();

builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(DisconnectingTrackingConstants.DISCONNECTED_CIRCUIT_RETENTION_PERIOD);
});

builder.Services.AddHostedService<GameTrackingService>();

builder.Services.AddHostedService<PlayerTrackingService>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<GameHub>(GameHub.HubUrl);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
