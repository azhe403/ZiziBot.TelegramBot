using Telegram.Bot;
using ZiziBot.TelegramBot.Attributes;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class SampleCommands : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task PingCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Message.Chat.Id, "Pong!");
    }

    [Command("start")]
    public async Task StartCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Message.Chat.Id, "Let's start!");
    }

    [TextCommand("mulai")]
    public async Task MulaiCommand(CommandData data)
    {
        await data.BotClient.SendTextMessageAsync(data.Message.Chat.Id, "Mari kita mulai!");
    }
}