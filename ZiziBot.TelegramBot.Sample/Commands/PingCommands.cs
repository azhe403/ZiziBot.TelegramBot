using Telegram.Bot;
using ZiziBot.TelegramBot.Attributes;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class PingCommands : BotCommandController
{
    [Command("ping")]
    public void PingCommand()
    {
        // data.BotClient.SendTextMessageAsync(data.Message.Chat.Id, "Pong!");
    }
}