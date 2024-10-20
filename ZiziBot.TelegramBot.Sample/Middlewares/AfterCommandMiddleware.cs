using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Middlewares;

public class AfterCommandMiddleware : IAfterCommand
{
    public async Task ExecuteAsync(CommandData commandData)
    { }
}