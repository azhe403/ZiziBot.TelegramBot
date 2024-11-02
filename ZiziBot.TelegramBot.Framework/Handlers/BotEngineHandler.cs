using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ZiziBot.TelegramBot.Framework.Handlers;

public class BotEngineHandler(
    ILogger<BotEngineHandler> logger,
    IServiceProvider serviceProvider
)
{
    public async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var messageHandler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();

        logger.LogDebug("Receiving update engine. UpdateId: {UpdateId}, UpdateType: {UpdateType}", update.Id, update.Type);
        await messageHandler.Handle(botClient, update, default);
    }
}