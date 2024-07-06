using JetBrains.Annotations;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class UpdateAttribute(UpdateType updateType) : Attribute
{
    public UpdateType UpdateType { get; } = updateType;
}