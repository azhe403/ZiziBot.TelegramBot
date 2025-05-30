﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;
using ZiziBot.TelegramBot.Framework.Engines;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Constants;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class ClientExtension
{
    public static IServiceCollection AddZiziBotTelegramBot(this IServiceCollection services, BotEngineConfig? engineConfig = null)
    {
        services.AddSingleton(provider => {
            var botCommandCollection = new BotCommandCollection() {
                CommandTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(x => x.IsSubclassOf(typeof(BotCommandController)))
            };

            return botCommandCollection;
        });

        services.Scan(selector => selector.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(filter => filter.AssignableTo<IBeforeCommand>())
            .As<IBeforeCommand>()
            .WithTransientLifetime());

        services.Scan(selector => selector.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(filter => filter.AssignableTo<IAfterCommand>())
            .As<IAfterCommand>()
            .WithTransientLifetime());

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

        services.AddSingleton<BotEngineHandler>();
        services.AddSingleton<BotClientCollection>();
        services.AddScoped<BotUpdateHandler>();

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

    public static async Task<IApplicationBuilder> UseZiziBotTelegramBot(this IApplicationBuilder app)
    {
        app.StartWebhookModeInternal();

        _ = await app.StartTelegramBot();

        return app;
    }

    private static void StartWebhookModeInternal(this IApplicationBuilder app)
    {
        if (app is WebApplication webApplication)
        {
            webApplication.MapPost(ValueConst.WebHookPath + "/{botId}", async (
                HttpContext context,
                BotEngineHandler botEngine,
                BotClientCollection botClientCollection,
                string botId,
                Update update
            ) => {
                var client = botClientCollection.Items.FirstOrDefault(x => x.Client.BotId.ToString() == botId);
                if (client == null)
                {
                    await context.Response.WriteAsync("Bot Client not found!");
                    return;
                }

                await botEngine.UpdateHandler(client.Client, update, CancellationToken.None);

                await context.Response.WriteAsync("OK");
            }).ExcludeFromDescription();

            webApplication.MapGet(ValueConst.WebHookPath + "/{botId}", async (
                HttpContext context,
                BotClientCollection botClientCollection,
                string botId
            ) => {
                var client = botClientCollection.Items.FirstOrDefault(x => x.Client.BotId.ToString() == botId);
                if (client == null)
                {
                    await context.Response.WriteAsync("Bot Client not found!");
                    return;
                }

                await context.Response.WriteAsync($"Hi!, set this URL for WebHook for {botId}");
            });
        }
    }

    private static async Task<IApplicationBuilder> StartTelegramBot(this IApplicationBuilder app)
    {
        var botEngine = app.ApplicationServices.GetRequiredService<IBotEngine>();

        await botEngine.Start();

        return app;
    }
}