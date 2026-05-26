using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UUIDNext;
using ZiziBot.TelegramBot.Framework.Models.Configs;

namespace ZiziBot.TelegramBot.Framework.Models;

public partial class CommandContext
{
    public string? BotToken { get; internal set; }
    public ITelegramBotClient? BotClient { get; internal set; }
    public Update? Update { get; internal set; }
    public BotEngineConfig? EngineConfig { get; internal set; }

    #region Join Request

    public ChatJoinRequest? ChatJoinRequest => Update?.ChatJoinRequest;

    #endregion

    #region CallbackQuery

    public CallbackQuery? CallbackQuery => Update?.CallbackQuery;
    public string CallbackQueryId => CallbackQuery?.Id ?? string.Empty;
    public string CallbackQueryData => CallbackQuery?.Data ?? string.Empty;
    public string[] CallbackQueryDatas => CallbackQueryData.Split(" ");
    public string CallbackQueryCmd => CallbackQueryDatas.FirstOrDefault() ?? string.Empty;
    public string CallbackQueryParam => string.Join(" ", CallbackQueryDatas.Skip(1));

    #endregion

    #region InlineQuery

    public InlineQuery? InlineQuery => Update?.InlineQuery;
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

    #region Message

    public Message? Message => Update?.Message ?? Update?.EditedMessage ?? CallbackQuery?.Message;
    public int MessageId => Message?.MessageId ?? default;
    public string? MessageText => Message?.Text ?? Message?.Caption;
    public string[]? MessageTexts => MessageText?.Split(" ");
    public Message? ReplyToMessage => Message?.ReplyToMessage?.Type is not (MessageType.ForumTopicCreated or MessageType.ForumTopicEdited) ? Message?.ReplyToMessage : default;

    #endregion

    #region Command

    public string? Command => MessageTexts?.FirstOrDefault();
    public string? CommandParam => MessageText?.Replace(Command ?? string.Empty, string.Empty).Trim();

    #endregion

    #region Channel Post

    public Message? ChannelPost => Update?.ChannelPost ?? Update?.EditedChannelPost;
    public int ChannelPostId => ChannelPost?.MessageId ?? default;

    #endregion

    #region Topic

    public ForumTopicCreated? ForumTopicCreated => Message?.ForumTopicCreated;
    public ForumTopicEdited? ForumTopicEdited => Message?.ForumTopicEdited;
    public string? EditedTopicName => ReplyToMessage?.ForumTopicEdited?.Name ?? ForumTopicEdited?.Name;
    public string? CreatedTopicName => ReplyToMessage?.ForumTopicCreated?.Name ?? ForumTopicCreated?.Name;
    public string? TopicName => EditedTopicName ?? CreatedTopicName;

    #endregion

    #region Boolean

    public bool IsChannel => Update?.Type is UpdateType.ChannelPost or UpdateType.EditedChannelPost;
    public bool IsPublicChat => Chat?.Type is ChatType.Group or ChatType.Supergroup;
    public bool IsPrivateChat => Chat?.Type == ChatType.Private;
    public bool IsSenderChat => Chat?.Type == ChatType.Sender;

    #endregion

    #region Timestamp

    public string SessionId { get; init; } = Uuid.NewSequential().ToString();
    public DateTime RequestDate { get; init; } = DateTime.UtcNow;

    #endregion
}