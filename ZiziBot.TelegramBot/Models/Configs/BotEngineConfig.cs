using ZiziBot.TelegramBot.Models.Enums;

namespace ZiziBot.TelegramBot.Models.Configs;

public class BotEngineConfig
{
    public const string ConfigPath = "BotEngine";

    public string WebhookUrl { get; set; }
    public BotEngineMode EngineMode { get; set; }

    public BotTokenConfig[] Bot { get; set; }
}