using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Engines;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Constants;
using ZiziBot.TelegramBot.Framework.Models.Enums;
using ZiziBot.TelegramBot.Framework.Workers;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class ClientExtension
{
    public static IServiceCollection AddZiziBotTelegramBot(this IServiceCollection services, BotEngineConfig? engineConfig = default)
    {
        services.AddSingleton(provider => {
            var botCommandCollection = new BotCommandCollection() {
                CommandTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(x => x.IsSubclassOf(typeof(BotCommandController)))
            };

            return botCommandCollection;
        });

        using (var provider = services.BuildServiceProvider())
        {
            var hostingEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

            var internalEngineConfig = new BotEngineConfig();
            if (engineConfig == null)
            {
                var botConfigurations = new List<BotTokenConfig>();
                var configuration = provider.GetRequiredService<IConfiguration>();

                configuration.GetSection(BotTokenConfig.CONFIG_PATH).Bind(botConfigurations);
                configuration.GetSection(BotEngineConfig.CONFIG_PATH).Bind(internalEngineConfig);
                services.AddSingleton(botConfigurations);
                services.AddSingleton(internalEngineConfig);
            }
            else
            {
                if (engineConfig.Bot == null)
                    throw new ArgumentNullException(nameof(engineConfig.Bot));

                services.AddSingleton(engineConfig.Bot);
                services.AddSingleton(engineConfig);
                internalEngineConfig = engineConfig;
            }


            switch (internalEngineConfig.EngineMode)
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
        }

        services.AddScoped<BotMessageHandler>();
        services.AddScoped<BotClientCollection>();

        return services;
    }

    private static IServiceCollection EnablePollingEngine(this IServiceCollection services)
    {
        services.AddScoped<IBotEngine, BotPollingEngine>();

        return services;
    }

    private static IServiceCollection EnableWebhookEngine(this IServiceCollection services)
    {
        services.AddScoped<IBotEngine, BotWebhookEngine>();

        return services;
    }

    public async static Task<IApplicationBuilder> UseZiziBotTelegramBot(this WebApplication app)
    {
        app.StartWebhookModeInternal();

        _ = await app.StartPollingModeInternal();

        return app;
    }

    static void StartWebhookModeInternal(this WebApplication app)
    {
        app.MapPost(ValueConst.WebHookPath + "/{botId}", async (
            HttpContext context,
            ILogger<BotWebhookEngine> logger,
            BotMessageHandler botMessageHandler,
            BotClientCollection botClientCollection,
            string botId,
            Update update
        ) => {
            var client = botClientCollection.Items.First(x => x.Client.BotId.ToString() == botId);

            logger.LogDebug("Receiving update webhook engine. UpdateId: {UpdateId}", update.Id);

            await botMessageHandler.Handle(client.Client, update, default);
            await context.Response.WriteAsync("OK");
        });
    }

    async static Task<IApplicationBuilder> StartPollingModeInternal(this IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();
        var botEngine = scope.ServiceProvider.GetRequiredService<IBotEngine>();

        await botEngine.Start();

        return app;
    }
}