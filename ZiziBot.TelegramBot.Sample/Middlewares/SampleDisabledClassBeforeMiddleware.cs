using ZiziBot.TelegramBot.Framework.Delegates;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

public class SampleDisabledClassBeforeMiddleware : IBeforeCommand
{
    public Task ExecuteAsync(CommandData commandData, CommandDelegate next)
    {
        throw new NotImplementedException();
    }
}