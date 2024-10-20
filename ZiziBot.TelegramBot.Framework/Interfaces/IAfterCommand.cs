using JetBrains.Annotations;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.Interfaces;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IAfterCommand
{
    Task ExecuteAsync(CommandData commandData);
}