using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class InlineQueryAttribute(string? command = default) : Attribute
{
    public string? Command => command;
}