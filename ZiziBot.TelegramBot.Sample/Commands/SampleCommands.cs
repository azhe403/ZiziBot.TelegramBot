using Telegram.Bot;
using Telegram.Bot.Types.Enums;
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
}