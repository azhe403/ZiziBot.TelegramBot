using JetBrains.Annotations;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class UpdateCommandAttribute(UpdateType updateType) : Attribute
{
    public UpdateType UpdateType { get; } = updateType;
}