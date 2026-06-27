using System.Text.RegularExpressions;

namespace ZiziBot.TelegramBot.Framework.Helpers;

public static partial class RegexHelper
{
    [GeneratedRegex(@"^\d+:[A-Za-z0-9_-]{35}$")]
    public static partial Regex BotTokenRegex();

    public static bool IsValidBotToken(string token)
    {
        return BotTokenRegex().IsMatch(token);
    }
}
