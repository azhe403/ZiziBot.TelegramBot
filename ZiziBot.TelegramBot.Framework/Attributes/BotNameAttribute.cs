namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BotNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}