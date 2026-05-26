using Telegram.Bot.Types.ReplyMarkups;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Sample.Commands;

public class SampleCommands : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task PingCommand()
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

        await SendMessage("Pong!", replyMarkup: replyMarkup);
    }

    [Command("start")]
    public async Task StartCommand()
    {
        await SendMessage("Let's start!");
    }

    [Command("say")]
    public async Task SayCommand()
    {
        await SendMessage($"You say: {Context.CommandParam}!");
    }

    [DefaultCommand]
    public async Task DefaultCommand()
    {
        await SendMessage("This is a default Command!");
    }

    [Callback("ping")]
    public async Task PingCallback()
    {
        await AnswerCallbackQuery("User clicked Ping!");
    }

    [Callback]
    public async Task DefaultCallback()
    {
        var text = $"Cmd: {Context.CallbackQueryCmd}" +
                   $"\nParam: {Context.CallbackQueryParam}";
        await AnswerCallbackQuery(text, showAlert: true);
    }
}