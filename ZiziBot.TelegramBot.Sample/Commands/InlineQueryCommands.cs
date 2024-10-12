using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class InlineQueryCommands : BotCommandController
{
    [InlineQuery]
    public async Task InlineQueryCommand(CommandData data)
    {
        await data.BotClient.AnswerInlineQueryAsync(data.InlineQueryId, new List<InlineQueryResult>() {
            new InlineQueryResultArticle("158cd95a-8f02-4a64-838e-78eab0fd53ac", "Default Inline", new InputTextMessageContent("Default Inline Content"))
        });
    }

    [InlineQuery("hello")]
    public async Task InlineQueryCommandHello(CommandData data)
    {
        await data.BotClient.AnswerInlineQueryAsync(data.InlineQueryId, new List<InlineQueryResult>() {
            new InlineQueryResultArticle("aa576dec-0727-4ea1-99ae-3c7cb20ea3c8", "Hello Inline", new InputTextMessageContent("Hello Inline Content"))
        });
    }
}