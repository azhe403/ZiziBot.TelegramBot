using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Handlers;

public class BotEngineHandler(
    ILogger<BotEngineHandler> logger,
    IServiceProvider serviceProvider,
    BotEngineConfig engineConfig
)
{
    public async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var updateHandlerInternal = UpdateHandlerInternal(botClient, update, token);

        if (engineConfig.ExecutionStrategy == ExecutionStrategy.Await)
            await updateHandlerInternal;
        else if (engineConfig.ExecutionStrategy == ExecutionStrategy.Background)
            _ = updateHandlerInternal;
    }

    private async Task UpdateHandlerInternal(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var messageHandler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();

        logger.LogDebug("Receiving update engine. UpdateId: {UpdateId}, UpdateType: {UpdateType}", update.Id, update.Type);
        await messageHandler.Handle(botClient, update, token);
    }
}