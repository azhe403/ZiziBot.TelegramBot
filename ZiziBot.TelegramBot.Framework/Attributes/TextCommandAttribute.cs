using JetBrains.Annotations;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class TextCommandAttribute(string command, ComparisonMode comparisonMode = ComparisonMode.Match) : Attribute
{
    public string Command { get; } = command;
    public ComparisonMode ComparisonMode { get; } = comparisonMode;
}