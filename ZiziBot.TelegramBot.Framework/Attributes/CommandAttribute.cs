using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class CommandAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}