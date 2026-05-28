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
        try
        {
            var updateHandlerInternal = UpdateHandlerInternal(botClient, update, token);

            if (engineConfig.ExecutionMode == ExecutionMode.Await)
            {
                await updateHandlerInternal;
            }
            else if (engineConfig.ExecutionMode == ExecutionMode.Background)
            {
                _ = updateHandlerInternal.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        logger.LogError(t.Exception, "Error handling update in background. UpdateId: {UpdateId}", update.Id);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling update. UpdateId: {UpdateId}", update.Id);
            throw;
        }
    }

    private async Task UpdateHandlerInternal(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var messageHandler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();

        await messageHandler.HandleUpdate(botClient, update, token);
    }
}
