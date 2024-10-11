using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;

namespace ZiziBot.TelegramBot.Framework.Workers;

public class BotEngineWorker(
    ILogger<BotEngineWorker> logger,
    List<BotTokenConfig> botConfigurations,
    IBotEngine botEngine
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var clients = botConfigurations.Select(x => BotClientItem.Create(x.Name, new TelegramBotClientOptions(x.Token)));
            await botEngine.Start();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Bot");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //todo.stop engine
        return Task.CompletedTask;
    }
}