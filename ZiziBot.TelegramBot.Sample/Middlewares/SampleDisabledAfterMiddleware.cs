using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

[DisabledMiddleware]
public class SampleDisabledAfterMiddleware : IAfterCommand
{
    public Task ExecuteAsync(CommandData commandData)
    {
        throw new NotImplementedException();
    }
}