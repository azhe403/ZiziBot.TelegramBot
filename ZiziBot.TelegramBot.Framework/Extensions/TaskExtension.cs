using Microsoft.Extensions.Logging;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class TaskExtension
{
    /// <summary>
    /// Attaches a fault-only continuation that logs exceptions, without blocking the caller.
    /// Intended for best-effort background tasks where failures should be observed and reported.
    /// </summary>
    public static void FireAndForget(this Task task, ILogger? logger = null)
    {
        _ = task.ContinueWith(t =>
        {
            if (t.Exception != null)
                logger?.LogError(t.Exception, "An unobserved task exception occurred.");
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }
}
