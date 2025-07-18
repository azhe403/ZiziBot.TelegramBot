using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Constants;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class EndpointExtension
{
    internal static void StartWebhookModeInternal(this IApplicationBuilder app)
    {
        if (app is not WebApplication webApplication)
            return;

        var webhookGroup = webApplication.MapGroup(ValueConst.WebHookPath).WithTags("ZiziBot.Webhook.Endpoint");

        webhookGroup.MapPost("{botToken}", async (
            HttpContext context,
            BotEngineHandler botEngine,
            BotClientCollection botClientCollection,
            string botToken,
            Update update
        ) =>
        {
            var bot = botClientCollection.Items.FirstOrDefault(x => x.BotToken == botToken);

            if (bot == null)
            {
                await context.Response.WriteAsync("Bot Client not found!");
                return;
            }

            await botEngine.UpdateHandler(bot.Client, update, CancellationToken.None);

            await context.Response.WriteAsync("OK");
        });

        webhookGroup.MapGet("{botToken}", async (
            HttpContext context,
            BotClientCollection botClientCollection,
            string botToken
        ) =>
        {
            var bot = botClientCollection.Items.FirstOrDefault(x => x.BotToken == botToken);

            if (bot == null)
            {
                await context.Response.WriteAsync("Bot Client not found!");
                return;
            }

            var me = await bot.Client.GetMe();
            await context.Response.WriteAsync($"Hi!, please set this URL for WebHook for {me.Username} for activate");
        });
    }
}