using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;

namespace ZiziBot.TelegramBot.Framework.Engines;

public class BotPollingEngine(
    ILogger<BotPollingEngine> logger,
    List<BotTokenConfig> botTokenConfigs,
    BotClientCollection botClientCollection,
    BotEngineHandler botEngineHandler
) : IBotEngine
{
    /// <summary>
    /// Starts long-polling for a single bot client and registers it in the shared collection.
    /// </summary>
    public async Task Start(BotClientItem clients)
    {
        logger.LogInformation("Starting polling for bot: {Name}", clients.Name);
        try
        {
            if (botClientCollection.ContainsName(clients.Name))
            {
                logger.LogWarning("Bot polling engine is already running");
                return;
            }

            await clients.Client.DeleteWebhook();

            clients.Client.StartReceiving(
                botEngineHandler.UpdateHandler,
                ErrorHandler,
                new ReceiverOptions(),
                clients.CancellationTokenSource?.Token ?? CancellationToken.None);

            _ = botClientCollection.TryAdd(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Polling bot. Name: {Name}", clients.Name);
        }
    }

    public async Task Start()
    {
        logger.LogDebug("Starting polling engine...");

        foreach (var botTokenConfig in botTokenConfigs)
        {
            try
            {
                var options = new TelegramBotClientOptions(botTokenConfig.Token);
                var client = BotClientItem.Create(botTokenConfig.Name, options);
                await Start(client);
            }
            catch (ArgumentException argumentException) when (argumentException.Message.Contains("Bot token invalid"))
            {
                logger.LogError("Bot token invalid for bot: {BotName}. Please check your configuration.", botTokenConfig.Name);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while starting bot: {BotName}", botTokenConfig.Name);
            }
        }
    }

    public Task Stop(string name)
    {
        if (!botClientCollection.TryRemoveByName(name, out var client))
            return Task.CompletedTask;

        client!.CancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public async Task Stop(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            await Stop(name);
        }
    }

    /// <summary>
    /// Stops all registered bots for the polling engine.
    /// </summary>
    public async Task StopEngine()
    {
        logger.LogDebug("Stopping polling engine...");

        var botNames = botClientCollection.GetNamesSnapshot();
        await Stop(botNames);
    }

    private Task ErrorHandler(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken token)
    {
        logger.LogError(exception, "Bot polling engine error. Source: {Source}", errorSource);
        return Task.CompletedTask;
    }
}