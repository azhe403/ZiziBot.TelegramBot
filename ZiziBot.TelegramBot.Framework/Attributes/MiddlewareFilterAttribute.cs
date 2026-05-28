using System;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MiddlewareFilterAttribute(UpdateType updateType) : Attribute
{
    public UpdateType UpdateType { get; } = updateType;
}