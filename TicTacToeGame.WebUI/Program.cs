using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.Services.RoomServices;
using TicTacToeGame.WebUI.Components;
using TicTacToeGame.WebUI.Components.Account;
using TicTacToeGame.WebUI.Data;
using TicTacToeGame.WebUI.Hubs;
using TicTacToeGame.WebUI.Services;

/*  // TODO:
        GamesStatistics:
         ? Create procedure where I just enters user id and it returns all his games

        Game:
         - Make that player can`t go on game just by url
         - Divide players into groups and make that they can`t see each other moves
         - I think MediatR should be added to the game         

        HostRoom:
         - Fix bug that when you refresh Room connection Id loads twice
         - Add service that will return unique Id according to that he already generated        

        JoinRoom:
        - Fix bug with timers that when message comes before timer elapsed - timer elapsing will also affect
        new comming messages

        Db:
        - Add to the procedures not taking that entities that are soft deleted

        Global: 
        - Add error handling to the pages (I think it should be SignalR that will return to specific page some error)
        - Fix slow loading game component (I think it is because if intervals of waiting of Polly in repositories)
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

builder.Services.AddSingleton<RoomService>();

builder.Services.AddScoped<IGamesStatisticsService, GamesStatisticsService>();

builder.Services.AddSingleton<IEmailSender<Player>, IdentityNoOpEmailSender>();

builder.Services.AddSingleton<RoomBackgroundService>();

builder.Services.AddScoped<Game>();


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
