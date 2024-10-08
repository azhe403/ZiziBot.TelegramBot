﻿using JetBrains.Annotations;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class TypedCommandAttribute(MessageType messageType) : Attribute
{
    public MessageType MessageType { get; } = messageType;
}