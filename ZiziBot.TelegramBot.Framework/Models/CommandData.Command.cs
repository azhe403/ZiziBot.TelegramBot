namespace ZiziBot.TelegramBot.Framework.Models;

public partial class CommandData
{
    public string? GetCommand(bool withoutSlash = false, bool withoutUsername = true)
    {
        var cmd = string.Empty;

        if (!MessageText?.StartsWith('/') ?? true)
            return cmd;

        cmd = MessageTexts?.ElementAtOrDefault(0);

        if (withoutSlash)
            cmd = cmd?.TrimStart('/');

        if (withoutUsername)
            cmd = cmd?.Split("@").FirstOrDefault();

        return cmd;
    }

    public bool IsCommand(string command)
    {
        return GetCommand() == command;
    }
}