using ZiziBot.TelegramBot.Engines;
using ZiziBot.TelegramBot.Handlers;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;
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

        services.AddSingleton<BotMessageHandler>();
        services.AddSingleton<BotClientCollection>();
        services.AddSingleton<IBotEngine, BotPollingEngine>();

        services.AddHostedService<BotPollingEngineWorker>();

        return services;
    }
}