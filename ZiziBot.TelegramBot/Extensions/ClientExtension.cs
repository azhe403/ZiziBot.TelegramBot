﻿using ZiziBot.TelegramBot.Engines;
using ZiziBot.TelegramBot.Handlers;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;
using ZiziBot.TelegramBot.Models.Configs;
using ZiziBot.TelegramBot.Workers;

namespace ZiziBot.TelegramBot.Extensions;

public static class ClientExtension
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services)
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
            var botConfigurations = new List<BotConfig>();
            var configuration = provider.GetRequiredService<IConfiguration>();

            configuration.GetSection(BotConfig.ConfigPath).Bind(botConfigurations);
            configuration.GetSection(BotEngineConfig.ConfigPath).Bind(engineConfig);

            services.AddSingleton(engineConfig);
            services.AddSingleton(botConfigurations);
        }

        services.AddSingleton<BotMessageHandler>();
        services.AddSingleton<BotClientCollection>();
        services.AddSingleton<IBotEngine, BotPollingEngine>();

        services.AddHostedService<BotPollingEngineWorker>();

        return services;
    }
}