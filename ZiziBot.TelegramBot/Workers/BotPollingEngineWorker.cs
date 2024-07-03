using Telegram.Bot;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Workers;

public class BotPollingEngineWorker(
    ILogger<BotPollingEngineWorker> logger,
    List<BotConfiguration> botConfigurations,
    IBotEngine botEngine
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var clients = botConfigurations.Select(x => BotClientItem.Create(x.Name, new TelegramBotClientOptions(x.Token)));
            botEngine.Start(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Polling bot");
            throw;
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //todo.stop engine
        return Task.CompletedTask;
    }
}