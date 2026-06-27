using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.HealthChecks;

public class BotConnectionHealthCheck(
    BotClientCollection botClientCollection,
    ILogger<BotConnectionHealthCheck> logger
) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (botClientCollection.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No bot clients configured");
            }

            var unhealthyBots = new List<string>();
            var healthyBots = new List<string>();

            foreach (var botClientItem in botClientCollection.GetAll())
            {
                try
                {
                    // Test bot connection by getting bot info
                    var botUser = await botClientItem.Client.GetMe(cancellationToken);
                    healthyBots.Add($"{botClientItem.Name} (@{botUser.Username})");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Health check failed for bot {BotName}", botClientItem.Name);
                    unhealthyBots.Add(botClientItem.Name);
                }
            }

            if (unhealthyBots.Count > 0)
            {
                return HealthCheckResult.Degraded(
                    $"Some bots are unhealthy: {string.Join(", ", unhealthyBots)}",
                    data: new Dictionary<string, object>
                    {
                        ["HealthyBots"] = healthyBots,
                        ["UnhealthyBots"] = unhealthyBots
                    });
            }

            return HealthCheckResult.Healthy(
                $"All {healthyBots.Count} bot(s) are connected",
                data: new Dictionary<string, object>
                {
                    ["HealthyBots"] = healthyBots
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bot connection health check failed");
            return HealthCheckResult.Unhealthy("Health check failed", exception: ex);
        }
    }
}
