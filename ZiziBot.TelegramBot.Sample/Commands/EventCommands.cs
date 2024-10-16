using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class EventCommands : BotCommandController
{
    [TypedCommand(MessageType.NewChatMembers)]
    public async Task NewChatMembersCommand(CommandData data)
    {
        await data.SendMessageText("Halo!");
    }

    [UpdateCommand(UpdateType.ChatJoinRequest)]
    public async Task ChatJoinRequestCommand(CommandData data)
    {
        await data.SendMessageText("Chat join request!");
    }
}