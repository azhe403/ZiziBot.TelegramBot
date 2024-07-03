namespace ZiziBot.TelegramBot.Models;

public class BotConfiguration
{
    public const string ConfigPath = "TelegramBot:BotConfiguration";

    public string Name { get; set; }
    public string Token { get; set; }
}