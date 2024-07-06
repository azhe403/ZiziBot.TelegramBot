using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TypedCommandAttribute(MessageType messageType) : Attribute
{
    public MessageType MessageType { get; } = messageType;
}