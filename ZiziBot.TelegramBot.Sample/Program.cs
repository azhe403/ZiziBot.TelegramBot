using ZiziBot.TelegramBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddZiziBotTelegramBot();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseZiziBotTelegramBot();

app.MapGet("/", () => "OK!");

await app.RunAsync();