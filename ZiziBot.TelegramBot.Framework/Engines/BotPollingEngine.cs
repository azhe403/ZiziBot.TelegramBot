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
    public async Task Start(BotClientItem clients)
    {
        logger.LogInformation("Starting polling for bot: {Name}", clients.Name);
        try
        {
            if (botClientCollection.Items.Exists(x => x.Name == clients.Name))
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
            botClientCollection.Items.Add(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Polling bot. Name: {Name}", clients.Name);
        }
    }

    public async Task Start()
    {
        logger.LogDebug("Starting polling engine...");
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

        client.CancellationTokenSource?.Cancel();
        botClientCollection.Items.Remove(client);
        return Task.CompletedTask;
    }

    public async Task Stop(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            await Stop(name);
        }
    }

    public async Task StopEngine()
    {
        logger.LogDebug("Stopping polling engine...");

        var botNames = botClientCollection.Items.Select(x => x.Name).ToList();
        await Stop(botNames);
    }

    private Task ErrorHandler(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken token)
    {
        logger.LogError(exception, "Bot polling engine error. Source: {Source}", errorSource);
        return Task.CompletedTask;
    }
}