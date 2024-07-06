using System.Reflection;
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
    public async Task<object?> OnUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        return update.Type switch
        {
            UpdateType.Message => await OnMessage(botClient, update.Message!, token),
            UpdateType.EditedMessage => await OnMessage(botClient, update.EditedMessage!, token),
            _ => default
        };
    }

    private async Task<object?> OnMessage(ITelegramBotClient botClient, Message updateMessage, CancellationToken token)
    {
        try
        {
            var invokeMethod = await InvokeMethod(botClient, UpdateType.Message, updateMessage);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return default;
    }

    private async Task<object?> InvokeMethod(ITelegramBotClient client, UpdateType type, Message message)
    {
        var method = GetMethod(type, message);

        if (method is { ControllerType: not null })
        {
            List<object> paramList = [];
            var methodParams = method.Method.GetParameters();

            if (methodParams.Any(x => x.ParameterType == typeof(CommandData)))
            {
                paramList =
                [
                    new CommandData()
                    {
                        BotClient = client,
                        Message = message,
                        Chat = message.Chat,
                        FromUser = message.From,
                        Params = method.Params
                    }
                ];
            }

            var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, method.ControllerType);

            return await MethodHelper.InvokeMethod(method.Method, paramList, controller);
        }

        return default;
    }

    private BotCommandInfo? GetMethod(UpdateType type, Message message)
    {
        var commands = GetMethods();
        var method = GetMethodByPath(commands, message);

        if (method.Item1 != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = GetCommands().Single(x => x == method.Item1.DeclaringType),
                Method = method.Item1,
                Params = method.Item2
            };
        }

        return default;
    }

    private (MethodInfo method, string) GetMethodByPath(IEnumerable<MethodInfo> methods, Message message)
    {
        var method = methods.FirstOrDefault(x =>
            x.GetCustomAttributes<CommandAttribute>().Any(a => message.Text?.Equals($"/{a.Path}") ?? false) ||
            x.GetCustomAttributes<TextCommandAttribute>().Any(a => message.Text?.Equals(a.Command) ?? false)
        );

        return method != null ?
            (method, string.Join(" ", message.Text!.Split(" ").Skip(1))) :
            default;
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