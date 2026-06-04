using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Constants;
using ZiziBot.TelegramBot.Framework.Models.Configs;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class EndpointExtension
{
    /// <summary>
    /// Maps webhook endpoints when running in webhook mode.
    /// If <see cref="BotEngineConfig.WebhookKey"/> is configured, requests must include it in the route.
    /// </summary>
    internal static void StartWebhookModeInternal(this IApplicationBuilder app)
    {
        if (app is not WebApplication webApplication)
            return;

        var webhookGroup = webApplication.MapGroup(ValueConst.WebHookPath).WithTags("ZiziBot.Webhook.Endpoint");

        // Shared handler for the "no-key" route variant.
        static async Task HandleWebhook(
            HttpContext context,
            BotEngineHandler botEngine,
            BotClientCollection botClientCollection,
            BotEngineConfig botEngineConfig,
            string bot,
            Update update
        )
        {
            if (!string.IsNullOrWhiteSpace(botEngineConfig.WebhookKey))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Not Found");
                return;
            }

            // When WebhookKey is not used, accept either token or name as route identifier for backward compatibility.
            if (!botClientCollection.TryGetByToken(bot, out var client) &&
                !botClientCollection.TryGetByName(bot, out client))
            {
                client = null;
            }

            if (client == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Bot Client not found!");
                return;
            }

            await botEngine.UpdateHandler(client.Client, update, context.RequestAborted);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("OK");
        }

        webhookGroup.MapPost("{bot}", async (
            HttpContext context,
            BotEngineHandler botEngine,
            BotClientCollection botClientCollection,
            BotEngineConfig botEngineConfig,
            string bot,
            Update update
        ) =>
        {
            await HandleWebhook(context, botEngine, botClientCollection, botEngineConfig, bot, update);
        });

        webhookGroup.MapPost("{webhookKey}/{bot}", async (
            HttpContext context,
            BotEngineHandler botEngine,
            BotClientCollection botClientCollection,
            BotEngineConfig botEngineConfig,
            string webhookKey,
            string bot,
            Update update
        ) =>
        {
            // Hide whether a webhook is configured by always returning 404 for invalid/missing keys.
            if (string.IsNullOrWhiteSpace(botEngineConfig.WebhookKey) || webhookKey != botEngineConfig.WebhookKey)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Not Found");
                return;
            }

            // When key is enabled, the bot route segment can be token or name depending on configuration.
            if (!botClientCollection.TryGetByToken(bot, out var client) &&
                !botClientCollection.TryGetByName(bot, out client))
            {
                client = null;
            }

            if (client == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Bot Client not found!");
                return;
            }

            await botEngine.UpdateHandler(client.Client, update, context.RequestAborted);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("OK");
        });

        webhookGroup.MapGet("{bot}", async (
            HttpContext context,
            BotClientCollection botClientCollection,
            BotEngineConfig botEngineConfig,
            string bot
        ) =>
        {
            // Keep the discovery endpoint disabled when the webhook key is enabled.
            if (!string.IsNullOrWhiteSpace(botEngineConfig.WebhookKey))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Not Found");
                return;
            }

            if (!botClientCollection.TryGetByToken(bot, out var client) &&
                !botClientCollection.TryGetByName(bot, out client))
            {
                client = null;
            }

            if (client == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Bot Client not found!");
                return;
            }

            var me = await client.Client.GetMe();
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync($"Hi!, please set this URL for WebHook for {me.Username} for activate");
        });
    }
}
