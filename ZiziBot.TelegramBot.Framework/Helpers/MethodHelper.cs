using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZiziBot.TelegramBot.Framework.Helpers;

public static class MethodHelper
{
    private static bool IsAsyncMethod(MethodInfo method)
    {
        return method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }

    public async static Task<object?> InvokeMethod(MethodInfo method, List<object> parameters, object instance)
    {
        if (IsAsyncMethod(method))
        {
            if (method.ReturnType == typeof(Task))
            {
                await ((Task)method.Invoke(instance, parameters.ToArray()))!;
                return null;
            }

            return await ((Task<object?>)method.Invoke(instance, parameters.ToArray()))!;
        }

        return method.Invoke(instance, parameters.ToArray());
    }
}