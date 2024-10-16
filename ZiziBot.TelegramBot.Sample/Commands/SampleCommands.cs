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
        await data.SendMessageText("Pong!");
    }

    [Command("start")]
    public async Task StartCommand(CommandData data)
    {
        await data.SendMessageText("Let's start!");
    }

    [Command("say")]
    public async Task SayCommand(CommandData data)
    {
        await data.SendMessageText($"You say: {data.CommandParam}!");
    }

    [DefaultCommand]
    public async Task DefaultCommand(CommandData data)
    {
        await data.SendMessageText("Default!");
    }
}