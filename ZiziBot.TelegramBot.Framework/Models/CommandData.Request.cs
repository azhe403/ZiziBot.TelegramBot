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
        int? messageThreadId = null,
        ParseMode parseMode = default,
        IEnumerable<MessageEntity>? entities = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        ReplyParameters? replyParameters = null,
        ReplyMarkup? replyMarkup = null,
        string? businessConnectionId = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(BotClient);
        ArgumentNullException.ThrowIfNull(Chat);
        ArgumentException.ThrowIfNullOrEmpty(text);

        replyParameters ??= new ReplyParameters() {
            AllowSendingWithoutReply = true
        };

        if (EngineConfig.ReplyMode == ReplyMode.ReplyToSender)
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
        int? cacheTime = null,
        bool isPersonal = false,
        string? nextOffset = null,
        InlineQueryResultsButton? button = null,
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

    public async Task AnswerCallbackQuery(
        string? text = null,
        bool showAlert = false,
        string? url = null,
        int? cacheTime = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(BotClient);

        await BotClient.AnswerCallbackQuery(CallbackQueryId, text, showAlert, url, cacheTime, cancellationToken);
    }
}