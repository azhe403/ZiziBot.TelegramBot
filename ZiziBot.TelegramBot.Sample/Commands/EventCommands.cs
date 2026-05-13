using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class EventCommands : BotCommandController
{
    [TypedCommand(MessageType.NewChatMembers)]
    public async Task NewChatMembersCommand()
    {
        await SendMessage("Halo!");
    }

    [UpdateCommand(UpdateType.ChatJoinRequest)]
    public async Task ChatJoinRequestCommand()
    {
        await SendMessage("Chat join request!");
    }
}