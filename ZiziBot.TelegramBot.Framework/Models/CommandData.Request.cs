using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Models;

public partial class CommandData
{
    public async Task<Message> SendMessage(
        string text,
        int? messageThreadId = default,
        ParseMode parseMode = default,
        IEnumerable<MessageEntity>? entities = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        ReplyParameters? replyParameters = default,
        IReplyMarkup? replyMarkup = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(BotClient);
        ArgumentNullException.ThrowIfNull(Chat);
        ArgumentException.ThrowIfNullOrEmpty(text);

        replyParameters ??= new ReplyParameters() {
            AllowSendingWithoutReply = true
        };

        if (EngineConfig.ReplyStrategy == ReplyStrategy.ReplyToSender)
        {
            replyParameters.MessageId = MessageId;
        }

        return await BotClient.SendMessage(
            chatId: Chat,
            text: text,
            messageThreadId: messageThreadId,
            parseMode: parseMode,
            entities: entities,
            linkPreviewOptions: linkPreviewOptions,
            disableNotification: disableNotification,
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            replyParameters: replyParameters,
            replyMarkup: replyMarkup,
            businessConnectionId: businessConnectionId,
            cancellationToken: cancellationToken
        );
    }

    public async Task AnswerInlineQuery(
        IEnumerable<InlineQueryResult> results,
        int? cacheTime = default,
        bool isPersonal = default,
        string? nextOffset = default,
        InlineQueryResultsButton? button = default,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(BotClient);
        ArgumentException.ThrowIfNullOrEmpty(InlineQueryId);

        await BotClient.AnswerInlineQuery(
            inlineQueryId: InlineQueryId,
            results: results,
            cacheTime: cacheTime,
            isPersonal: isPersonal,
            nextOffset: nextOffset,
            button: button,
            cancellationToken: cancellationToken
        );
    }
}