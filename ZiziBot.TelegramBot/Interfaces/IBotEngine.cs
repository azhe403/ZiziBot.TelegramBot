using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Interfaces;

public interface IBotEngine
{
    Task Start(BotClientItem clients);
    Task Start(IEnumerable<BotClientItem> clients);
    
    Task Stop(string name);
    Task Stop(IEnumerable<string> names);
}