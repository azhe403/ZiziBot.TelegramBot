namespace ZiziBot.TelegramBot.Models.Configs;

public class BotTokenConfig
{
    public const string ConfigPath = "BotEngine:Bot";

    public required string Name { get; set; }
    public required string Token { get; set; }
}