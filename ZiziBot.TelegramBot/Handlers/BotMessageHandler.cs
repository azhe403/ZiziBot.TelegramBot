﻿using System.Reflection;
using Allowed.Telegram.Bot.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Attributes;
using ZiziBot.TelegramBot.Models;

namespace ZiziBot.TelegramBot.Handlers;

public class BotMessageHandler(IServiceProvider provider, ILogger<BotMessageHandler> logger, BotCommandCollection commandCollection)
{
    public async Task<object?> Handle(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        return update.Type switch
        {
            UpdateType.Message => await OnMessage(botClient, update.Message!, token),
            UpdateType.EditedMessage => await OnMessage(botClient, update.EditedMessage!, token),
            _ => await OnUpdate(botClient, update, token)
        };
    }

    private async Task<object?> OnUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            var method = GetMethodByUpdate(update);
            var invokeMethod = await InvokeMethod(botClient, method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return default;
    }

    private async Task<object?> OnMessage(ITelegramBotClient botClient, Message updateMessage, CancellationToken token)
    {
        try
        {
            var method = GetMethodByPath(updateMessage);
            var invokeMethod = await InvokeMethod(botClient, method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return default;
    }

    private async Task<object?> InvokeMethod(ITelegramBotClient client, BotCommandInfo botCommandInfo)
    {
        if (botCommandInfo is { ControllerType: not null })
        {
            List<object> paramList = [];
            var methodParams = botCommandInfo.Method.GetParameters();

            if (methodParams.Any(x => x.ParameterType == typeof(CommandData)))
            {
                paramList =
                [
                    new CommandData()
                    {
                        BotClient = client,
                        Update = botCommandInfo.Update,
                        Message = botCommandInfo.Message,
                        Chat = botCommandInfo.Message?.Chat,
                        FromUser = botCommandInfo.Message?.From,
                        Params = botCommandInfo.Params
                    }
                ];
            }

            var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, botCommandInfo.ControllerType);

            return await MethodHelper.InvokeMethod(botCommandInfo.Method, paramList, controller);
        }

        return default;
    }

    private BotCommandInfo? GetMethodByUpdate(Update update)
    {
        var commands = GetMethods();
        var method = commands.FirstOrDefault(info => info.GetCustomAttributes<UpdateAttribute>().Any(a => a.UpdateType == update.Type));

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = GetCommands().Single(x => x == method.DeclaringType),
                Method = method,
                Update = update
            };
        }

        return default;
    }

    private BotCommandInfo? GetMethodByPath(Message message)
    {
        var methods = GetMethods();
        var method = methods.FirstOrDefault(x =>
            x.GetCustomAttributes<CommandAttribute>().Any(a => message.Text?.Equals($"/{a.Path}") ?? false) ||
            x.GetCustomAttributes<TextCommandAttribute>().Any(a => message.Text?.Equals(a.Command) ?? false) ||
            x.GetCustomAttributes<TypedCommandAttribute>().Any(a => message.Type == a.MessageType)
        );

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = GetCommands().Single(x => x == method.DeclaringType),
                Method = method,
                Message = message,
                Params = string.Join(" ", message.Text?.Split(" ").Skip(1) ?? Array.Empty<string>())
            };
        }

        return default;
    }

    private IEnumerable<MethodInfo> GetMethods()
    {
        return commandCollection.CommandTypes.SelectMany(x => x.GetMethods());
    }

    private IEnumerable<Type> GetCommands()
    {
        return commandCollection.CommandTypes;
    }
}