using Telegram.Bot;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Workers;

public class BotPollingEngineWorker(ILogger<BotPollingEngineWorker> logger, BotClientCollection botClientCollection, IBotEngine botEngine) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            botEngine.Start(new[]
            {
                BotClientItem.CreateClient("Main", new TelegramBotClientOptions("YOUR_BOT_TOKEN"))
            });
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