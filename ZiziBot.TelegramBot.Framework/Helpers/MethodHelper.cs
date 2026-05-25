using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZiziBot.TelegramBot.Framework.Helpers;

public static class MethodHelper
{
    private static bool IsAsyncMethod(MethodInfo method)
    {
        return method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
    }

    public static async Task<object?> InvokeMethod(MethodInfo method, List<object> parameters, object instance)
    {
        if (IsAsyncMethod(method))
        {
            if (method.ReturnType == typeof(Task))
            {
                var task = method.Invoke(instance, parameters.ToArray()) as Task;
                if (task != null) await task;
                return null;
            }

            var taskWithResult = method.Invoke(instance, parameters.ToArray()) as Task<object?>;
            if (taskWithResult != null) return await taskWithResult;

            return null;
        }

        return method.Invoke(instance, parameters.ToArray());
    }
}