namespace ZiziBot.TelegramBot.Models;

public class BotCommandController
{
    public virtual void Initialize(long telegramId)
    {
    }

    public virtual Task InitializeAsync(long telegramId)
    {
        return Task.CompletedTask;
    }
}