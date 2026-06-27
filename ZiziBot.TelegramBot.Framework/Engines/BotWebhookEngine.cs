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
    /// <summary>
    /// Registers the webhook for a single bot client and stores it in the shared collection.
    /// </summary>
    public async Task Start(BotClientItem clients)
    {
        logger.LogInformation("Starting webhook for bot: {Name}", clients.Name);
        try
        {
            if (botClientCollection.ContainsName(clients.Name))
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

            // Build webhook route as:
            // - /{WebHookPath}/{bot} when WebhookKey is not set
            // - /{WebHookPath}/{WebhookKey}/{bot} when WebhookKey is set
            var routePrefix = string.IsNullOrWhiteSpace(botEngineConfig.WebhookKey)
                ? $"{ValueConst.WebHookPath}"
                : $"{ValueConst.WebHookPath}/{botEngineConfig.WebhookKey}";

            // Avoid exposing the bot token in URLs by using the bot name as the path segment.
            var botSegment = botEngineConfig.UseBotTokenInWebhookPath ? clients.BotToken : clients.Name;
            var webhookUrl = $"{botEngineConfig.WebhookUrl}/{routePrefix}/{botSegment}";

            logger.LogDebug("Setting up Webhook for Bot {BotId}", clients.Client.BotId);

            await clients.Client.SetWebhook(webhookUrl);

            _ = botClientCollection.TryAdd(clients);
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
        if (!botClientCollection.TryRemoveByName(name, out var client))
            return Task.CompletedTask;

        return client!.Client.DeleteWebhook();
    }

    public Task Stop(IEnumerable<string> names)
    {
        return Task.WhenAll(names.Select(Stop));
    }

    /// <summary>
    /// Stops all registered bots for the webhook engine (only when configured for webhook mode).
    /// </summary>
    public async Task StopEngine()
    {
        logger.LogDebug("Stopping webhook engine...");

        if (botEngineConfig.EngineMode != BotEngineMode.Webhook)
        {
            logger.LogInformation("Bot engine is not in webhook mode, skipping webhook engine shutdown");
            return;
        }

        var botNames = botClientCollection.GetNamesSnapshot();
        await Stop(botNames);
    }
}
