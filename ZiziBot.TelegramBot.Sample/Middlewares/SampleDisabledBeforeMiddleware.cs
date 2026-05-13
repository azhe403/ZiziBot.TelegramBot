using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Delegates;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

[DisabledMiddleware]
public class SampleDisabledBeforeMiddleware : IBeforeCommand
{
    public Task ExecuteAsync(CommandContext commandContext, CommandDelegate next)
    {
        throw new NotImplementedException();
    }
}