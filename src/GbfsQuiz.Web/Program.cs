using FluentValidation;
using GbfsQuiz.Web.Common.Middleware;
using GbfsQuiz.Web.Common.Security;
using GbfsQuiz.Web.Common.Startup;
using GbfsQuiz.Web.Components;
using GbfsQuiz.Web.Components.State;
using GbfsQuiz.Web.Features.Auth;
using GbfsQuiz.Web.Features.Games;
using GbfsQuiz.Web.Features.Leaderboard;
using GbfsQuiz.Web.Features.Players;
using GbfsQuiz.Application;
using GbfsQuiz.Application.Features.Players.Interfaces;
using GbfsQuiz.Infrastructure;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGameRateLimiting();
builder.Services.AddValidatorsFromAssembly(typeof(GbfsQuiz.Application.DependencyInjection).Assembly);

// Blazor + MudBlazor UI (Interactive Server). Components call the application services
// directly; the HTTP API + JWT below remain for programmatic clients and tests.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
        options.DetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddMudServices();
builder.Services.AddScoped<PlayerSession>();
builder.Services.AddScoped<IAvatarStorage, FileSystemAvatarStorage>();

var app = builder.Build();

await app.MigrateDatabaseAsync();

// Pipeline: log -> catch -> harden, before routing/auth.
app.UseRequestLogging();
app.UseGlobalExceptionHandling();
app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Static assets for the Blazor UI (framework JS, MudBlazor bundle, app.css, download.js).
// MapStaticAssets (not UseStaticFiles) reads the published static-web-assets manifest, so
// _framework/blazor.web.js is served correctly in a Release/container publish, not just in dev.
app.MapStaticAssets();

// Runtime-uploaded avatars live under wwwroot/avatars/ and aren't in the build-time
// static-web-assets manifest, so the classic middleware serves them at request time.
app.UseStaticFiles();

app.MapAuthEndpoints();
app.MapGameEndpoints();
app.MapLeaderboardEndpoints();
app.MapPlayerEndpoints();

// Serve the Blazor (MudBlazor) UI.
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/// <summary>Exposed so the integration-test host (WebApplicationFactory) can reference it.</summary>
public partial class Program;
