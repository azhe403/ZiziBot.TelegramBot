using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Delegates;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

[MiddlewareFilter(UpdateType.Message)]
public class SampleMessageMiddleware(ILogger<SampleMessageMiddleware> logger) : IBeforeCommand
{
    public async Task ExecuteAsync(CommandContext commandContext, CommandDelegate next)
    {
        logger.LogInformation("This middleware is only executed for Message updates.");
        await next(commandContext);
    }
}