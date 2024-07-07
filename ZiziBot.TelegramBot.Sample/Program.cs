using System.Security.Cryptography.X509Certificates;
using Serilog;
using ZiziBot.TelegramBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(x => x
    .MinimumLevel.Debug()
    .WriteTo.Console());

builder.Services.AddZiziBotTelegramBot();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseZiziBotTelegramBot();

app.MapGet("/", () => "OK!");

await app.RunAsync();