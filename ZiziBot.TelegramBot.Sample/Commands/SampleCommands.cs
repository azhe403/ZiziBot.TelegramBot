using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class SampleCommands : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task PingCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Pong!");
    }

    [Command("start")]
    public async Task StartCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Let's start!");
    }

    [TextCommand("mulai")]
    public async Task MulaiCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Mari kita mulai!");
    }

    [DefaultCommand]
    public async Task DefaultCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Default!");
    }

    [TypedCommand(MessageType.NewChatMembers)]
    public async Task NewChatMembersCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Halo!");
    }

    [UpdateCommand(UpdateType.ChatJoinRequest)]
    public async Task ChatJoinRequestCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Chat, "Chat join request!");
    }

    [UpdateCommand(UpdateType.InlineQuery)]
    public async Task InlineQueryCommand(CommandData data)
    {
        await data.BotClient.AnswerInlineQueryAsync(data.InlineQueryId, new List<InlineQueryResult>()
        {
            new InlineQueryResultArticle("aa576dec-0727-4ea1-99ae-3c7cb20ea3c8", "Sample Inline", new InputTextMessageContent("Inline Content"))
        });
    }
}