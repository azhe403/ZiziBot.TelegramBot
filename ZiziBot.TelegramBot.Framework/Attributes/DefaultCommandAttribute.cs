﻿using JetBrains.Annotations;

namespace ZiziBot.TelegramBot.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class DefaultCommandAttribute : Attribute
{ }