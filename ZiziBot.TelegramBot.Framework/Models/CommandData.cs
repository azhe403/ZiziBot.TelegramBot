using Telegram.Bot;
using Telegram.Bot.Types;

namespace ZiziBot.TelegramBot.Framework.Models;

public class CommandData
{
    public required string BotToken { get; set; }
    public required ITelegramBotClient BotClient { get; set; }
    public required Update Update { get; set; }

    public CallbackQuery CallbackQuery { get; set; }

    public InlineQuery? InlineQuery => Update.InlineQuery;
    public string InlineQueryId => InlineQuery?.Id ?? string.Empty;

    public Message? Message { get; set; }
    public Chat? Chat => Update.ChatJoinRequest?.Chat ?? Message?.Chat;
    public User? From => Update.ChatJoinRequest?.From ?? Update.InlineQuery?.From ?? Message?.From;
    public string? Params { get; set; }
}