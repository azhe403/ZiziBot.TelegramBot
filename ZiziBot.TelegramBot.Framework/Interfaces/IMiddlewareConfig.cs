using ZiziBot.TelegramBot.Framework.Models.Configs;

namespace ZiziBot.TelegramBot.Framework.Interfaces;

public interface IMiddlewareConfig
{
    BotEngineConfig? Config { get; set; }
}