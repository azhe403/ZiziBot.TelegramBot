using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZiziBot.TelegramBot.Framework.Engines;
using ZiziBot.TelegramBot.Framework.Handlers;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class ClientExtension
{
    public static IServiceCollection AddZiziBotTelegramBot(this IServiceCollection services, BotEngineConfig? engineConfig = null)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddSingleton(provider =>
        {
            var botCommandCollection = new BotCommandCollection()
            {
                CommandTypes = assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(x => x.IsSubclassOf(typeof(BotCommandController)))
            };

            return botCommandCollection;
        });

        services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IBeforeCommand>())
            .As<IBeforeCommand>()
            .WithScopedLifetime());

        services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IAfterCommand>())
            .As<IAfterCommand>()
            .WithScopedLifetime());

        using var provider = services.BuildServiceProvider();

        var hostingEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

        var internalEngineConfig = new BotEngineConfig();

        if (engineConfig == null)
        {
            var botConfigurations = new List<BotTokenConfig>();
            var configuration = provider.GetRequiredService<IConfiguration>();

            configuration.GetSection(BotTokenConfig.CONFIG_PATH).Bind(botConfigurations);
            configuration.GetSection(BotEngineConfig.CONFIG_PATH).Bind(internalEngineConfig);

            services.AddSingleton(botConfigurations);
        }
        else
        {
            var configBot = engineConfig.Bot ?? throw new ApplicationException("Bot config is null");

            services.AddSingleton(configBot);
            internalEngineConfig = engineConfig;
        }


        switch (internalEngineConfig.EngineMode)
        {
            case BotEngineMode.Webhook:
                services.EnableWebhookEngine();
                internalEngineConfig.ActualEngineMode = BotEngineMode.Webhook;
                break;
            case BotEngineMode.Polling:
                services.EnablePollingEngine();
                internalEngineConfig.ActualEngineMode = BotEngineMode.Polling;
                break;
            case BotEngineMode.Auto:
            default:
            {
                if (hostingEnvironment.IsDevelopment())
                {
                    services.EnablePollingEngine();
                    internalEngineConfig.ActualEngineMode = BotEngineMode.Polling;
                }
                else
                {
                    services.EnableWebhookEngine();
                    internalEngineConfig.ActualEngineMode = BotEngineMode.Webhook;
                }

                break;
            }
        }

        services.AddSingleton(internalEngineConfig);
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

    private static async Task<IApplicationBuilder> StartTelegramBot(this IApplicationBuilder app)
    {
        var botEngine = app.ApplicationServices.GetRequiredService<IBotEngine>();

        await botEngine.Start();

        return app;
    }
}