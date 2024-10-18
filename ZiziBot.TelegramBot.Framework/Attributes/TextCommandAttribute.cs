using JetBrains.Annotations;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class TextCommandAttribute(string command, ComparisonType comparisonType = ComparisonType.Match) : Attribute
{
    public string Command { get; } = command;
    public ComparisonType ComparisonType { get; } = comparisonType;
}