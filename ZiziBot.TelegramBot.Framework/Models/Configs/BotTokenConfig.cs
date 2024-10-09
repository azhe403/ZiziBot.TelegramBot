namespace ZiziBot.TelegramBot.Framework.Models.Configs;

public class BotTokenConfig
{
    public const string CONFIG_PATH = "BotEngine:Bot";

    public required string Name { get; set; }
    public required string Token { get; set; }
}