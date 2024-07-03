using Telegram.Bot;

namespace ZiziBot.TelegramBot.Models;

public class BotClientItem
{
    public required string Name { get; set; }
    public required ITelegramBotClient Client { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public static BotClientItem Create(string name, TelegramBotClientOptions options, CancellationTokenSource? source = null)
    {
        source ??= new CancellationTokenSource();
        return new BotClientItem
        {
            Name = name,
            Client = new TelegramBotClient(options),
            CancellationTokenSource = source,
        };
    }
}