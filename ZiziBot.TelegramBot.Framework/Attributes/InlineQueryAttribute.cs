using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class InlineQueryAttribute(string? command = default) : Attribute
{
    public string? Command => command;
}