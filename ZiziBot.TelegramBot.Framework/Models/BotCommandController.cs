using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace ZiziBot.TelegramBot.Framework.Models;

public class BotCommandController
{
    public CommandContext Context { get; internal set; } = null!;

    public virtual void Initialize(long telegramId)
    {
    }

    public virtual Task InitializeAsync(long telegramId)
    {
        return Task.CompletedTask;
    }

    protected Task<Message> SendMessage(string text, ReplyMarkup? replyMarkup = null)
    {
        return Context.SendMessage(
            text: text,
            replyMarkup: replyMarkup,
            replyParameters: new ReplyParameters()
            {
                MessageId = Context.MessageId,
                AllowSendingWithoutReply = true
            });
    }

    protected Task AnswerCallbackQuery(string text, bool showAlert = false)
    {
        return Context.AnswerCallbackQuery(text, showAlert);
    }

    protected Task AnswerInlineQuery(List<InlineQueryResult> results)
    {
        return Context.AnswerInlineQuery(results);
    }
}