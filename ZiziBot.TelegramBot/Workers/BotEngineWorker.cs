﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZiziBot.TelegramBot.Interfaces;
using ZiziBot.TelegramBot.Models;
using ZiziBot.TelegramBot.Models.Configs;

namespace ZiziBot.TelegramBot.Workers;

public class BotEngineWorker(
    ILogger<BotEngineWorker> logger,
    List<BotTokenConfig> botConfigurations,
    IBotEngine botEngine
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var clients = botConfigurations.Select(x => BotClientItem.Create(x.Name, new TelegramBotClientOptions(x.Token)));
            botEngine.Start(clients);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error starting Bot");
            throw;
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //todo.stop engine
        return Task.CompletedTask;
    }
}