using Telegram.Bot;
using Telegram.Bot.Types;

namespace ZiziBot.TelegramBot.Models;

public class CommandData
{
    public ITelegramBotClient BotClient { get; set; }
    public Update Update { get; set; }
    public Message? Message { get; set; }
    public Chat? Chat { get; set; }
    public User? FromUser { get; set; }
    public string? Params { get; set; }
}