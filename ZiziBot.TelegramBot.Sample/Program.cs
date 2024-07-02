using ZiziBot.TelegramBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelegramBot();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => "OK!");
app.MapPost("api/telegram-webhook", context =>
{
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsync("OK");
});

await app.RunAsync();