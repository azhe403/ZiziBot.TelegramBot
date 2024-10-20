using ZiziBot.TelegramBot.Framework.Delegates;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

public class BeforeCommandMiddleware : IBeforeCommand
{
    public async Task ExecuteAsync(CommandData commandData, CommandDelegate next)
    {
        await next(commandData);
    }
}