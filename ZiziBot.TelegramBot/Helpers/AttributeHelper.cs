using System.Reflection;
using ZiziBot.TelegramBot.Attributes;

namespace ZiziBot.TelegramBot.Helpers;

public static class AttributeHelper
{
    public static IEnumerable<CommandAttribute> GetCommandAttributes(this MethodInfo method)
    {
        return (CommandAttribute[])method.GetCustomAttributes<CommandAttribute>(false);
    }
}