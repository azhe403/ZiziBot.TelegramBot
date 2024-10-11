using Serilog;
using ZiziBot.TelegramBot.Framework.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(x => x
    .MinimumLevel.Debug()
    .WriteTo.Console());

builder.Services.AddZiziBotTelegramBot();

var app = builder.Build();

app.UseHttpsRedirection();
await app.UseZiziBotTelegramBot();

app.MapGet("/", () => "OK!");

await app.RunAsync();