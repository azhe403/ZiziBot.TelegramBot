using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Models.Configs;

public class BotEngineConfig
{
    public const string CONFIG_PATH = "BotEngine";

    public string? WebhookUrl { get; set; }
    public BotEngineMode EngineMode { get; set; }

    public List<BotTokenConfig> Bot { get; set; }
}