using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Engines;
using ZiziBot.TelegramBot.Handlers;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;
using ZiziBot.TelegramBot.Models.Configs;
using ZiziBot.TelegramBot.Models.Constants;
using ZiziBot.TelegramBot.Models.Enums;
using ZiziBot.TelegramBot.Workers;

namespace ZiziBot.TelegramBot.Extensions;

public static class ClientExtension
{
    public static IServiceCollection AddZiziBotTelegramBot(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var botCommandCollection = new BotCommandCollection()
            {
                CommandTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(x => x.IsSubclassOf(typeof(BotCommandController)))
            };

            return botCommandCollection;
        });

        using (var provider = services.BuildServiceProvider())
        {
            var engineConfig = new BotEngineConfig();
            var botConfigurations = new List<BotTokenConfig>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var hostingEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

            configuration.GetSection(BotTokenConfig.ConfigPath).Bind(botConfigurations);
            configuration.GetSection(BotEngineConfig.ConfigPath).Bind(engineConfig);

            services.AddSingleton(engineConfig);
            services.AddSingleton(botConfigurations);

            switch (engineConfig.EngineMode)
            {
                case BotEngineMode.Webhook:
                    services.EnableWebhookEngine();
                    break;
                case BotEngineMode.Polling:
                    services.EnablePollingEngine();
                    break;
                case BotEngineMode.Auto:
                default:
                {
                    if (hostingEnvironment.IsDevelopment())
                        services.EnablePollingEngine();
                    else
                        services.EnableWebhookEngine();

                    break;
                }
            }

            services.AddHostedService<BotEngineWorker>();
        }

        services.AddSingleton<BotMessageHandler>();
        services.AddSingleton<BotClientCollection>();

        return services;
    }

    private static IServiceCollection EnablePollingEngine(this IServiceCollection services)
    {
        services.AddSingleton<IBotEngine, BotPollingEngine>();

        return services;
    }

    private static IServiceCollection EnableWebhookEngine(this IServiceCollection services)
    {
        services.AddSingleton<IBotEngine, BotWebhookEngine>();

        return services;
    }

    public static IApplicationBuilder UseZiziBotTelegramBot(this WebApplication app)
    {
        app.MapPost(ValueConst.WebHookPath + "/{botId}", async (
            HttpContext context,
            ILogger<BotWebhookEngine> logger,
            BotMessageHandler botMessageHandler,
            BotClientCollection botClientCollection,
            string botId,
            Update update
        ) =>
        {
            var client = botClientCollection.Items.First(x => x.Client.BotId.ToString() == botId);

            logger.LogDebug("Receiving update webhook engine. UpdateId: {UpdateId}", update.Id);

            await botMessageHandler.OnUpdate(client.Client, update, default);
            await context.Response.WriteAsync("OK");
        });

        return app;
    }
}