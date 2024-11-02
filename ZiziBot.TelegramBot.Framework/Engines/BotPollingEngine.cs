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
        try
        {
            if (botClientCollection.Items.Exists(x => x.Name == clients.Name))
            {
                logger.LogWarning("Bot polling engine is already running");
                return;
            }

            await clients.Client.DeleteWebhookAsync();

            clients.Client.StartReceiving(botEngineHandler.UpdateHandler, ErrorHandler);
            botClientCollection.Items.Add(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Polling bot. Name: {Name}", clients.Name);
        }
    }

    public async Task Start()
    {
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

    private Task ErrorHandler(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken token)
    {
        logger.LogError(exception, "Bot polling engine error. Source: {Source}", errorSource);
        return Task.CompletedTask;
    }
}