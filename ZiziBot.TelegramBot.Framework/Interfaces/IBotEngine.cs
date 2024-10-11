using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.Interfaces;

public interface IBotEngine
{
    Task Start(BotClientItem clients);
    Task Start();

    Task Stop(string name);
    Task Stop(IEnumerable<string> names);
}