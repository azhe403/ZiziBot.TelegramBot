using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class TextCommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}