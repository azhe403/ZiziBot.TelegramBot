using System.Reflection;

namespace ZiziBot.TelegramBot.Framework.Models;

public class BotCommandCollection
{
    public IEnumerable<Type> CommandTypes { get; set; } = [];
    public IReadOnlyList<MethodInfo> Methods { get; set; } = [];
}
