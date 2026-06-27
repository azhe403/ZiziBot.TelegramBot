using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using ZiziBot.TelegramBot.Framework.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(x => x
    .MinimumLevel.Verbose()
    .WriteTo.Console());

builder.Services.AddZiziBotTelegramBot();

// Add health checks
builder.Services.AddHealthChecks()
    .AddZiziBotTelegramBotHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();
await app.UseZiziBotTelegramBot();

app.MapGet("/", () => "OK!");

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "bot-connection"
});

await app.RunAsync();