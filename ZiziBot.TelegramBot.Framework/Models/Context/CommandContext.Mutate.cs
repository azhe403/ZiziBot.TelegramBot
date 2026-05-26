namespace ZiziBot.TelegramBot.Framework.Models;

public partial class CommandContext
{
    internal void SetContext(CommandContext commandContext)
    {
        BotToken = commandContext.BotToken;
        BotClient = commandContext.BotClient;
        Update = commandContext.Update;
        EngineConfig = commandContext.EngineConfig;
    }
}