namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TextCommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}