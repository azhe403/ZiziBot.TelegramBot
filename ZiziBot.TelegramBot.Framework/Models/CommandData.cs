using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ZiziBot.TelegramBot.Framework.Models;

public class CommandData
{
    public required string BotToken { get; init; }
    public required ITelegramBotClient BotClient { get; init; }
    public required Update Update { get; init; }

    #region Join Request
    public ChatJoinRequest? ChatJoinRequest => Update.ChatJoinRequest;
    #endregion

    #region CallbackQuery
    public CallbackQuery? CallbackQuery => Update.CallbackQuery;
    public string CallbackQueryId => CallbackQuery?.Id ?? string.Empty;
    #endregion

    #region InlineQuery
    public InlineQuery? InlineQuery => Update.InlineQuery;
    public string InlineQueryId => InlineQuery?.Id ?? string.Empty;
    public string InlineQueryQuery => InlineQuery?.Query ?? string.Empty;
    public string InlineQueryQueryCmd => InlineQueryQuery.Split(" ").FirstOrDefault() ?? string.Empty;
    public string InlineQueryQueryParam => string.Join(" ", InlineQueryQuery.Split(" ").Skip(1));
    public string InlineQueryOffset => InlineQuery?.Offset ?? string.Empty;
    #endregion

    #region Chat
    public Chat? Chat => ChatJoinRequest?.Chat ?? Message?.Chat;
    public ChatType ChatType => Message?.Chat.Type ?? default;
    public ChatId ChatId => Chat?.Id ?? default;
    public string ChatTitle => Chat?.Title ?? $"{User?.FirstName} {User?.LastName}".Trim();
    public string ChatUsername => Chat?.Username ?? string.Empty;
    public long ChatIdentifier => ChatId.Identifier ?? default;
    public bool ChatHasUsername => !string.IsNullOrEmpty(ChatUsername);
    #endregion

    #region User
    public User? User => ChatJoinRequest?.From ?? Message?.From ?? CallbackQuery?.From ?? InlineQuery?.From;
    public User? ReplyToUser => ReplyToMessage?.From;
    public long UserId => User?.Id ?? default;
    public string UserFirstName => User?.FirstName ?? string.Empty;
    public string UserLastName => User?.LastName ?? string.Empty;
    public string UserUsername => User?.Username ?? string.Empty;
    public string UserFullName => $"{User?.FirstName} {User?.LastName}".Trim();
    public string UserLanguageCode => User?.LanguageCode ?? string.Empty;
    public bool UserHasUsername => !string.IsNullOrEmpty(UserUsername);
    #endregion

    #region Channel Post
    public Message? ChannelPost => Update.ChannelPost ?? Update.EditedChannelPost;
    public int ChannelPostId => ChannelPost?.MessageId ?? default;
    #endregion

    #region Topic
    public ForumTopicCreated? ForumTopicCreated => Message?.ForumTopicCreated;
    public ForumTopicEdited? ForumTopicEdited => Message?.ForumTopicEdited;
    public string? EditedTopicName => ReplyToMessage?.ForumTopicEdited?.Name ?? ForumTopicEdited?.Name;
    public string? CreatedTopicName => ReplyToMessage?.ForumTopicCreated?.Name ?? ForumTopicCreated?.Name;
    public string? TopicName => EditedTopicName ?? CreatedTopicName;
    #endregion

    #region Message
    public Message? ReplyToMessage => Message?.ReplyToMessage?.Type is not (MessageType.ForumTopicCreated or MessageType.ForumTopicEdited) ? Message?.ReplyToMessage : default;
    public Message? Message => Update.Message ?? Update.EditedMessage ?? CallbackQuery?.Message;
    public string MessageId => Message?.MessageId.ToString() ?? string.Empty;
    public string MessageText => Message?.Text ?? string.Empty;
    public string[] MessageTexts => MessageText.Split(" ");
    #endregion

    #region Boolean
    public bool IsChannel => Update.Type is UpdateType.ChannelPost or UpdateType.EditedChannelPost;
    public bool IsPublicChat => Chat?.Type is ChatType.Group or ChatType.Supergroup;
    public bool IsPrivateChat => Chat?.Type == ChatType.Private;
    public bool IsSenderChat => Chat?.Type == ChatType.Sender;
    #endregion

    #region Timestamp
    public string TransactionId => Guid.NewGuid().ToString();
    public DateTime RequestDate => DateTime.UtcNow;
    #endregion

    public string? CommandParam { get; set; }
}