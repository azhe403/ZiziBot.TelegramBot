using System.Reflection;
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
using ZiziBot.TelegramBot.Framework.Validation;

namespace ZiziBot.TelegramBot.Framework.Extensions;

public static class ClientExtension
{
    /// <summary>
    /// Registers framework services, command discovery, and engine configuration.
    /// </summary>
    public static IServiceCollection AddZiziBotTelegramBot(this IServiceCollection services, BotEngineConfig? engineConfig = null)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.AddSingleton(_ =>
        {
            // Returns all loadable types for an assembly (skips dynamic assemblies and tolerates partial load failures).
            static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
            {
                if (assembly.IsDynamic)
                    return [];

                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null).Cast<Type>();
                }
            }

            var commandTypes = assemblies
                .SelectMany(GetLoadableTypes)
                .Where(x => x.IsSubclassOf(typeof(BotCommandController)))
                .ToArray();

            var methods = commandTypes
                .SelectMany(x => x.GetMethods())
                .ToArray();

            return new BotCommandCollection
            {
                CommandTypes = commandTypes,
                Methods = methods
            };
        });

        services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IBeforeCommand>())
            .As<IBeforeCommand>()
            .WithScopedLifetime());

        services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IAfterCommand>())
            .As<IAfterCommand>()
            .WithScopedLifetime());

        if (engineConfig == null)
        {
            services.AddSingleton(sp =>
            {
                var botConfigurations = new List<BotTokenConfig>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                configuration.GetSection(BotTokenConfig.ConfigPath).Bind(botConfigurations);
                return botConfigurations;
            });

            services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var hostingEnvironment = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ClientExtension).FullName ?? nameof(ClientExtension));

                var internalEngineConfig = new BotEngineConfig();

                configuration.GetSection(BotEngineConfig.ConfigPath).Bind(internalEngineConfig);

                internalEngineConfig.ActualEngineMode = internalEngineConfig.EngineMode switch
                {
                    BotEngineMode.Webhook => BotEngineMode.Webhook,
                    BotEngineMode.Polling => BotEngineMode.Polling,
                    _ => hostingEnvironment.IsDevelopment() ? BotEngineMode.Polling : BotEngineMode.Webhook
                };

                logger.LogInformation("Bot engine mode is {EngineMode}, actual mode is {ActualEngineMode}", internalEngineConfig.EngineMode, internalEngineConfig.ActualEngineMode);

                return internalEngineConfig;
            });
        }
        else
        {
            var configBot = engineConfig.Bot ?? throw new ApplicationException("Bot config is null");

            services.AddSingleton(configBot);
            services.AddSingleton(sp =>
            {
                var hostingEnvironment = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ClientExtension).FullName ?? nameof(ClientExtension));

                engineConfig.ActualEngineMode = engineConfig.EngineMode switch
                {
                    BotEngineMode.Webhook => BotEngineMode.Webhook,
                    BotEngineMode.Polling => BotEngineMode.Polling,
                    _ => hostingEnvironment.IsDevelopment() ? BotEngineMode.Polling : BotEngineMode.Webhook
                };

                logger.LogInformation("Bot engine mode is {EngineMode}, actual mode is {ActualEngineMode}", engineConfig.EngineMode, engineConfig.ActualEngineMode);

                return engineConfig;
            });
        }

        services.AddSingleton<BotPollingEngine>();
        services.AddSingleton<BotWebhookEngine>();
        services.AddSingleton<IBotEngine>(sp =>
        {
            var config = sp.GetRequiredService<BotEngineConfig>();
            return config.ActualEngineMode == BotEngineMode.Polling
                ? sp.GetRequiredService<BotPollingEngine>()
                : sp.GetRequiredService<BotWebhookEngine>();
        });

        services.AddSingleton<BotEngineHandler>();
        services.AddSingleton<BotClientCollection>();
        services.AddScoped<BotUpdateHandler>();
        services.AddScoped<CommandContext>();
        services.AddSingleton<BotEngineConfigValidator>();

        return services;
    }

    public static async Task<IApplicationBuilder> UseZiziBotTelegramBot(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<BotEngineConfig>();
        var validator = app.ApplicationServices.GetRequiredService<BotEngineConfigValidator>();

        // Validate configuration before starting
        validator.Validate(config);

        if (config.ActualEngineMode == BotEngineMode.Webhook)
            app.StartWebhookModeInternal();

        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            var botEngine = app.ApplicationServices.GetRequiredService<IBotEngine>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IBotEngine>>();
            logger.LogInformation("Stopping bot engine gracefully");
            botEngine.StopEngine().GetAwaiter().GetResult();
        });

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
