using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZiziBot.TelegramBot.Framework.HealthChecks;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class HealthChecksExtension
{
    /// <summary>
    /// Adds ZiziBot Telegram Bot health checks to the service collection.
    /// </summary>
    public static IHealthChecksBuilder AddZiziBotTelegramBotHealthChecks(
        this IHealthChecksBuilder builder,
        string? botConnectionName = null,
        string? webhookName = null)
    {
        var connectionName = botConnectionName ?? "bot-connection";
        var webhookHealthName = webhookName ?? "bot-webhook";

        builder.AddCheck<BotConnectionHealthCheck>(connectionName);
        builder.AddCheck<BotWebhookHealthCheck>(webhookHealthName);

        return builder;
    }
}
