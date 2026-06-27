using Microsoft.Extensions.Logging;
using ZiziBot.TelegramBot.Framework.Helpers;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Validation;

public class BotEngineConfigValidator(ILogger<BotEngineConfigValidator> logger)
{
    public void Validate(BotEngineConfig config)
    {
        var errors = new List<string>();

        // Validate bot tokens
        if (config.Bot == null || config.Bot.Count == 0)
        {
            errors.Add("At least one bot token must be configured under BotEngine:Bot");
        }
        else
        {
            ValidateBotTokens(config.Bot, errors);
        }

        // Validate webhook configuration
        if (config.ActualEngineMode == BotEngineMode.Webhook)
        {
            ValidateWebhookConfig(config, errors);
        }

        if (errors.Count > 0)
        {
            var errorMessage = "Bot engine configuration validation failed:\n" + string.Join("\n", errors);
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        logger.LogInformation("Bot engine configuration validation passed");
    }

    private void ValidateBotTokens(List<BotTokenConfig> botConfigs, List<string> errors)
    {
        var botNames = new HashSet<string>();

        for (var i = 0; i < botConfigs.Count; i++)
        {
            var botConfig = botConfigs[i];
            var prefix = $"BotEngine:Bot[{i}]";

            // Validate name
            if (string.IsNullOrWhiteSpace(botConfig.Name))
            {
                errors.Add($"{prefix}: Name is required");
            }
            else if (botNames.Contains(botConfig.Name))
            {
                errors.Add($"{prefix}: Name '{botConfig.Name}' is not unique");
            }
            else
            {
                botNames.Add(botConfig.Name);
            }

            // Validate token
            if (string.IsNullOrWhiteSpace(botConfig.Token))
            {
                errors.Add($"{prefix}: Token is required");
            }
            else if (!RegexHelper.IsValidBotToken(botConfig.Token))
            {
                errors.Add($"{prefix}: Token format is invalid (should match pattern: \\d+:[A-Za-z0-9_-]+)");
            }
        }
    }

    private void ValidateWebhookConfig(BotEngineConfig config, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            errors.Add("BotEngine:WebhookUrl is required when EngineMode is Webhook");
        }
        else if (!IsValidUrl(config.WebhookUrl))
        {
            errors.Add("BotEngine:WebhookUrl must be a valid HTTPS URL");
        }
        else if (!config.WebhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("BotEngine:WebhookUrl must use HTTPS");
        }
    }



    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
