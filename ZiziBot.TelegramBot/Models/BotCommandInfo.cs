using System.Reflection;

namespace ZiziBot.TelegramBot.Models;

public class BotCommandInfo
{
    public Type ControllerType { get; set; }
    public MethodInfo Method { get; set; }
    public string Params { get; set; }
}