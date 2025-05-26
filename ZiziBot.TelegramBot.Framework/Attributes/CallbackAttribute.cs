using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class CallbackAttribute(string? command = null) : Attribute
{
    public string? Command => command;
}