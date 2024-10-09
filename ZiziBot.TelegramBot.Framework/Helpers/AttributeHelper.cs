using System.Reflection;
using ZiziBot.TelegramBot.Framework.Attributes;

namespace ZiziBot.TelegramBot.Framework.Helpers;

public static class AttributeHelper
{
    public static IEnumerable<CommandAttribute> GetCommandAttributes(this MethodInfo method)
    {
        return (CommandAttribute[])method.GetCustomAttributes<CommandAttribute>(false);
    }
}