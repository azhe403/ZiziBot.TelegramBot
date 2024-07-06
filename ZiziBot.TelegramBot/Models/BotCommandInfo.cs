using System.Reflection;
using Telegram.Bot.Types;

namespace ZiziBot.TelegramBot.Models;

public class BotCommandInfo
{
    public required Type ControllerType { get; set; }
    public required MethodInfo Method { get; set; }
    public Update? Update { get; set; }
    public Message? Message { get; set; }
    public string? Params { get; set; }
}