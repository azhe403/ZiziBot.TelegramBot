using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        var logger = provider.GetRequiredService<ILogger<BotEngineHandler>>();

        var internalEngineConfig = new BotEngineConfig();

        if (engineConfig == null)
        {
            var botConfigurations = new List<BotTokenConfig>();
            var configuration = provider.GetRequiredService<IConfiguration>();

            configuration.GetSection(BotTokenConfig.ConfigPath).Bind(botConfigurations);
            configuration.GetSection(BotEngineConfig.ConfigPath).Bind(internalEngineConfig);

            services.AddSingleton(botConfigurations);
        }
        else
        {
            var configBot = engineConfig.Bot ?? throw new ApplicationException("Bot config is null");

            services.AddSingleton(configBot);
            internalEngineConfig = engineConfig;
        }

        logger.LogInformation("Bot engine mode is {EngineMode}", internalEngineConfig.EngineMode);


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
                    logger.LogInformation("Starting bot in Polling mode because of Development environment");
                    services.EnablePollingEngine();
                    internalEngineConfig.ActualEngineMode = BotEngineMode.Polling;
                }
                else
                {
                    logger.LogInformation("Starting bot in Webhook mode because of non-Development environment");
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
        services.AddScoped<CommandContext>();

        return services;
    }

    private static IServiceCollection EnablePollingEngine(this IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<BotPollingEngine>>();

        logger.LogInformation("Enabling Polling engine");
        services.AddSingleton<IBotEngine, BotPollingEngine>();

        return services;
    }

    private static IServiceCollection EnableWebhookEngine(this IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<BotWebhookEngine>>();

        logger.LogInformation("Enabling Webhook engine");
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
        var logger = app.ApplicationServices.GetRequiredService<ILogger<IBotEngine>>();

        logger.LogInformation("Starting bot engine");
        await botEngine.Start();

        return app;
    }
}