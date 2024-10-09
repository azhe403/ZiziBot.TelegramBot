namespace ZiziBot.TelegramBot.Framework.Models;

public class BotCommandCollection
{
    public IEnumerable<Type> CommandTypes { get; set; } = [];
}