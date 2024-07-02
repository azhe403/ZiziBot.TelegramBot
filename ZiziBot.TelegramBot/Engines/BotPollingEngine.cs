using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Handlers;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Engines;

public class BotPollingEngine(ILogger<BotPollingEngine> logger, BotClientCollection botClientCollection, BotMessageHandler botMessageHandler) : IBotEngine
{
    public async Task Start(BotClientItem clients)
    {
        if (botClientCollection.Items.Exists(x => x.Name == clients.Name))
        {
            logger.LogWarning("Bot polling engine is already running");
            return;
        }

        await clients.Client.DeleteWebhookAsync();

        clients.Client.StartReceiving(UpdateHandler, ErrorHandler);
        botClientCollection.Items.Add(clients);
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

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        logger.LogDebug("Receiving update polling engine. UpdateId: {UpdateId}", update.Id);
        await botMessageHandler.OnUpdate(botClient, update, token);
    }
}