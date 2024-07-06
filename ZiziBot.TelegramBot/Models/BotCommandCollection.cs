namespace ZiziBot.TelegramBot.Models;

public class BotCommandCollection
{
    public IEnumerable<Type> CommandTypes { get; set; } = [];
}