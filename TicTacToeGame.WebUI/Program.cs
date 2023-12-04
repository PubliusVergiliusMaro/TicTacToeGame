using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.WebUI.Components;
using TicTacToeGame.WebUI.Components.Account;
using TicTacToeGame.WebUI.Data;
using TicTacToeGame.WebUI.Hubs;
using TicTacToeGame.WebUI.Services.RoomBackgroundServices;

/*  // TODO:
        GamesStatistics:
         
         ? Create procedure where I just enters user id and it returns all his games

        Game:

         ? And when user back to the game - he should see the game state

         ? Add Board To Db

        Global: 

        - Add error handling to the pages (I think it should be SignalR that will return to specific page some error)
        
        ? Fix slow loading game component (I think it is because if intervals of waiting of Polly in repositories)
 */


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

builder.Services.AddScoped<IGamesStatisticsService, GamesStatisticsService>();

builder.Services.AddSingleton<IEmailSender<Player>, IdentityNoOpEmailSender>();

builder.Services.AddSingleton<RoomBackgroundService>();

builder.Services.AddScoped<GameInitializeProcess>();

builder.Services.AddScoped<CheckForWinnerManager>();

builder.Services.AddScoped<MakeMovesGameManager>();

builder.Services.AddTransient<PlayerDisconectingTrackingService>();

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

app.MapHub<GameHub>("/gamehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
