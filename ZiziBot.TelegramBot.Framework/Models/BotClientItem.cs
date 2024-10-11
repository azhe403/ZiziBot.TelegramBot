using Telegram.Bot;

namespace ZiziBot.TelegramBot.Framework.Models;

public class BotClientItem
{
    public required string Name { get; init; }
    public required string BotToken { get; init; }
    public required ITelegramBotClient Client { get; init; }
    public CancellationTokenSource? CancellationTokenSource { get; init; }

    public static BotClientItem Create(string name, TelegramBotClientOptions options, CancellationTokenSource? source = null)
    {
        source ??= new CancellationTokenSource();
        return new BotClientItem {
            Name = name,
            BotToken = options.Token,
            Client = new TelegramBotClient(options),
            CancellationTokenSource = source,
        };
    }
}