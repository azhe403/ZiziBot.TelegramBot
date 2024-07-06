using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Attributes;

public class UpdateAttribute(UpdateType updateType):Attribute
{
    public UpdateType UpdateType { get; } = updateType;
}