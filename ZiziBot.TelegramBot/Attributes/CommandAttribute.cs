namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}