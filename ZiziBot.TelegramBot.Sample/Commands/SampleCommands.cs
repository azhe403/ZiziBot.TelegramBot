using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class SampleCommands : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task PingCommand(CommandData data)
    {
        var replyMarkup = new InlineKeyboardMarkup([
            [
                new InlineKeyboardButton("Ping") {
                    CallbackData = "ping"
                },
                new InlineKeyboardButton("Foo Bar") {
                    CallbackData = "foo bar"
                },
                new InlineKeyboardButton("lorem ipsum") {
                    CallbackData = "lorem ipsum dolor sit amet"
                }
            ]
        ]);

        await data.SendMessage("Pong!", replyMarkup: replyMarkup);
    }

    [Command("start")]
    public async Task StartCommand(CommandData data)
    {
        await data.SendMessage("Let's start!");
    }

    [Command("say")]
    public async Task SayCommand(CommandData data)
    {
        await data.SendMessage($"You say: {data.CommandParam}!");
    }

    [DefaultCommand]
    public async Task DefaultCommand(CommandData data)
    {
        await data.SendMessage("Default!");
    }

    [Callback("ping")]
    public async Task PingCallback(CommandData data)
    {
        await data.AnswerCallbackQuery("User clicked Ping!");
    }

    [Callback]
    public async Task DefaultCallback(CommandData data)
    {
        await data.AnswerCallbackQuery($"Cmd: {data.CallbackQueryCmd}, Param: {data.CallbackQueryParam}");
    }
}