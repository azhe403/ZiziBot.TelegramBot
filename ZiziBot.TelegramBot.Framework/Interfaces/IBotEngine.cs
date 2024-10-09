using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.Interfaces;

public interface IBotEngine
{
    Task Start(BotClientItem clients);
    Task Start(IEnumerable<BotClientItem> clients);

    Task Stop(string name);
    Task Stop(IEnumerable<string> names);
}