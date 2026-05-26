using Telegram.Bot.Types.InlineQueryResults;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class InlineQueryCommands(CommandContext context) : BotCommandController
{
    [InlineQuery]
    public async Task InlineQueryCommand()
    {
        await AnswerInlineQuery(new List<InlineQueryResult>()
        {
            new InlineQueryResultArticle("default-inline", "Default Inline", new InputTextMessageContent("Default Inline Content")),
            new InlineQueryResultArticle("hello-inline", "Hello Inline", new InputTextMessageContent("Hello Inline Content"))
        });
    }

    [InlineQuery("hello")]
    public async Task InlineQueryCommandHello()
    {
        await AnswerInlineQuery(new List<InlineQueryResult>()
        {
            new InlineQueryResultArticle("aa576dec-0727-4ea1-99ae-3c7cb20ea3c8", "Hello Inline", new InputTextMessageContent("Hello Inline Content"))
        });
    }

    [InlineQuery("id")]
    public async Task InlineQueryCommandId()
    {
        var message = $"ID: {Context.UserId}" +
                      $"\nUsername: {Context.UserUsername}";

        await AnswerInlineQuery(new List<InlineQueryResult>()
        {
            new InlineQueryResultArticle("aa576dec-0727-4ea1-99ae-3c7cb20ea3c8", "Select to see your ID", new InputTextMessageContent(message))
        });
    }
}