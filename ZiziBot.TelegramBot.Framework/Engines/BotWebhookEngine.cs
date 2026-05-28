using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Constants;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Engines;

public class BotWebhookEngine(
    ILogger<BotWebhookEngine> logger,
    List<BotTokenConfig> botTokenConfigs,
    BotClientCollection botClientCollection,
    BotEngineConfig botEngineConfig
) : IBotEngine
{
    public async Task Start(BotClientItem clients)
    {
        logger.LogInformation("Starting webhook for bot: {Name}", clients.Name);
        try
        {
            if (botClientCollection.Items.Exists(x => x.Name == clients.Name))
            {
                logger.LogWarning("Bot webhook engine is already running for bot: {Name}", clients.Name);
                return;
            }

            await clients.Client.DeleteWebhook();

            if (string.IsNullOrWhiteSpace(botEngineConfig.WebhookUrl))
            {
                logger.LogError("WebhookUrl is not configured. Path: {ConfigPath}:WebhookUrl", BotEngineConfig.ConfigPath);
                return;
            }

            var webhookUrl = botEngineConfig.WebhookUrl + "/" + ValueConst.WebHookPath + "/" + clients.BotToken;
            logger.LogDebug("Setting up Webhook url for Bot {BotId} to {WebhookUrl}", clients.Client.BotId, webhookUrl);

            await clients.Client.SetWebhook(webhookUrl);

            botClientCollection.Items.Add(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Webhook bot. Name: {Name}", clients.Name);
        }
    }

    public async Task Start()
    {
        logger.LogDebug("Starting webhook engine...");
        var clients = botTokenConfigs.Select(x => BotClientItem.Create(x.Name, new TelegramBotClientOptions(x.Token)));

        foreach (var client in clients)
        {
            await Start(client);
        }
    }

    public Task Stop(string name)
    {
        var client = botClientCollection.Items.Find(x => x.Name == name);
        
        if (client == null)
            return Task.CompletedTask;

        botClientCollection.Items.Remove(client);
        
        return client.Client.DeleteWebhook();
    }

    public Task Stop(IEnumerable<string> names)
    {
        return Task.WhenAll(names.Select(Stop));
    }

    public async Task StopEngine()
    {
        logger.LogDebug("Stopping webhook engine...");

        if (botEngineConfig.EngineMode != BotEngineMode.Webhook)
        {
            logger.LogInformation("Bot engine is not in webhook mode, skipping webhook engine shutdown");
            return;
        }

        var botNames = botClientCollection.Items.Select(x => x.Name).ToList();
        await Stop(botNames);
    }
}