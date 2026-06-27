using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.HealthChecks;

public class BotWebhookHealthCheck(
    BotClientCollection botClientCollection,
    BotEngineConfig botEngineConfig,
    ILogger<BotWebhookHealthCheck> logger
) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsWebhookMode())
            {
                return HealthCheckResult.Healthy("Webhook mode not enabled, skipping webhook health check");
            }

            if (botClientCollection.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No bot clients configured");
            }

            var (webhookInfoList, botsWithWebhookIssues) = await CheckAllBotsWebhooks(cancellationToken);

            return BuildHealthCheckResult(webhookInfoList, botsWithWebhookIssues);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook health check failed");
            return HealthCheckResult.Unhealthy("Webhook health check failed", exception: ex);
        }
    }

    private bool IsWebhookMode()
    {
        return botEngineConfig.ActualEngineMode == BotEngineMode.Webhook;
    }

    private async Task<(List<Dictionary<string, object>> webhookInfoList, List<string> botsWithWebhookIssues)> CheckAllBotsWebhooks(
        CancellationToken cancellationToken)
    {
        var webhookInfoList = new List<Dictionary<string, object>>();
        var botsWithWebhookIssues = new List<string>();

        foreach (var botClientItem in botClientCollection.GetAll())
        {
            var (webhookData, issue) = await CheckSingleBotWebhook(botClientItem, cancellationToken);
            webhookInfoList.Add(webhookData);

            if (issue != null)
            {
                botsWithWebhookIssues.Add(issue);
            }
        }

        return (webhookInfoList, botsWithWebhookIssues);
    }

    private async Task<(Dictionary<string, object> webhookData, string? issue)> CheckSingleBotWebhook(
        BotClientItem botClientItem,
        CancellationToken cancellationToken)
    {
        try
        {
            var webhookInfo = await botClientItem.Client.GetWebhookInfo(cancellationToken);

            var webhookData = BuildWebhookData(botClientItem, webhookInfo);
            var issue = DetectWebhookIssue(botClientItem, webhookInfo);

            return (webhookData, issue);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook health check failed for bot {BotName}", botClientItem.Name);
            var errorData = BuildErrorWebhookData(botClientItem);
            var issue = $"{botClientItem.Name}: Failed to get webhook info";
            return (errorData, issue);
        }
    }

    private Dictionary<string, object> BuildWebhookData(BotClientItem botClientItem, WebhookInfo webhookInfo)
    {
        return new Dictionary<string, object>
        {
            ["BotName"] = botClientItem.Name,
            ["Url"] = webhookInfo.Url ?? "Not set",
            ["PendingUpdateCount"] = webhookInfo.PendingUpdateCount,
            ["LastError"] = webhookInfo.LastErrorDate != null
                ? $"{webhookInfo.LastErrorMessage} (at {webhookInfo.LastErrorDate})"
                : "None",
            ["MaxConnections"] = webhookInfo.MaxConnections ?? 40
        };
    }

    private Dictionary<string, object> BuildErrorWebhookData(BotClientItem botClientItem)
    {
        return new Dictionary<string, object>
        {
            ["BotName"] = botClientItem.Name,
            ["Url"] = "Not set",
            ["PendingUpdateCount"] = 0,
            ["LastError"] = "Failed to get webhook info",
            ["MaxConnections"] = 40
        };
    }

    private string? DetectWebhookIssue(BotClientItem botClientItem, WebhookInfo webhookInfo)
    {
        if (string.IsNullOrWhiteSpace(webhookInfo.Url))
        {
            return $"{botClientItem.Name}: Webhook URL not set";
        }

        if (webhookInfo.PendingUpdateCount > 10)
        {
            return $"{botClientItem.Name}: High pending update count ({webhookInfo.PendingUpdateCount})";
        }

        if (!string.IsNullOrWhiteSpace(webhookInfo.LastErrorMessage))
        {
            return $"{botClientItem.Name}: Last error - {webhookInfo.LastErrorMessage}";
        }

        return null;
    }

    private HealthCheckResult BuildHealthCheckResult(
        List<Dictionary<string, object>> webhookInfoList,
        List<string> botsWithWebhookIssues)
    {
        if (botsWithWebhookIssues.Count > 0)
        {
            return HealthCheckResult.Degraded(
                "Some webhooks have issues",
                data: new Dictionary<string, object>
                {
                    ["Issues"] = botsWithWebhookIssues,
                    ["WebhookInfo"] = webhookInfoList
                });
        }

        return HealthCheckResult.Healthy(
            $"All {webhookInfoList.Count} webhook(s) are healthy",
            data: new Dictionary<string, object>
            {
                ["WebhookInfo"] = webhookInfoList
            });
    }
}
