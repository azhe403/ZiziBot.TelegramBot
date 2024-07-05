using Telegram.Bot;
using ZiziBot.TelegramBot.Handlers;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;
using ZiziBot.TelegramBot.Models.Configs;
using ZiziBot.TelegramBot.Models.Constants;

namespace ZiziBot.TelegramBot.Engines;

public class BotWebhookEngine(
    ILogger<BotWebhookEngine> logger,
    BotClientCollection botClientCollection,
    BotEngineConfig botEngineConfig,
    BotMessageHandler botMessageHandler
) : IBotEngine
{
    public async Task Start(BotClientItem clients)
    {
        try
        {
            if (botClientCollection.Items.Exists(x => x.Name == clients.Name))
            {
                logger.LogWarning("Bot polling engine is already running");
                return;
            }

            await clients.Client.DeleteWebhookAsync();

            var webhookUrl = botEngineConfig.WebhookUrl + "/" + ValueConst.WebHookPath + "/" + clients.Client.BotId;

            await clients.Client.SetWebhookAsync(webhookUrl);

            botClientCollection.Items.Add(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Polling bot. Name: {Name}", clients.Name);
        }
    }

    public async Task Start(IEnumerable<BotClientItem> clients)
    {
        foreach (var client in clients)
        {
            await Start(client);
        }
    }

    public Task Stop(string name)
    {
        throw new NotImplementedException();
    }

    public Task Stop(IEnumerable<string> names)
    {
        throw new NotImplementedException();
    }
}