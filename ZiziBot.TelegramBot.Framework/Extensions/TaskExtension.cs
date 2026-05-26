using Microsoft.Extensions.Logging;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class TaskExtension
{
    public static void FireAndForget(this Task task, ILogger? logger = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                logger?.LogError(t.Exception, "An unobserved task exception occurred.");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}