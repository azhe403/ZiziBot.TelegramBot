using JetBrains.Annotations;
using ZiziBot.TelegramBot.Framework.Delegates;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.Interfaces;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IBeforeCommand
{
    Task ExecuteAsync(CommandData commandData, CommandDelegate next);
}