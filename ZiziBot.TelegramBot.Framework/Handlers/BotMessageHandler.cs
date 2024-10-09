using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Helpers;
using ZiziBot.TelegramBot.Framework.Models;

namespace ZiziBot.TelegramBot.Framework.Handlers;

public class BotMessageHandler(
    IServiceProvider provider,
    ILogger<BotMessageHandler> logger,
    BotCommandCollection commandCollection
)
{
    public async Task<object?> Handle(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var result = update.Type switch {
            UpdateType.Message => await OnMessage(botClient, update, token),
            UpdateType.EditedMessage => await OnMessage(botClient, update, token),
            _ => await OnUpdate(botClient, update, token)
        };

        return result;
    }

    private async Task<object?> OnUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            var method = GetMethod(update);
            var invokeMethod = await InvokeMethod(botClient, method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return default;
    }

    private async Task<object?> OnMessage(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            var message = update.Message ?? update.EditedMessage;
            var method = GetMethod(message!);
            if (method == null)
            {
                logger.LogDebug("No handler for this update: {Update}", update.Id);
                return default;
            }

            method.Update = update;

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
                paramList = [
                    new CommandData() {
                        BotClient = client,
                        Update = botCommandInfo.Update,
                        Message = botCommandInfo.Message,
                        Params = botCommandInfo.Params
                    }
                ];
            }

            var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, botCommandInfo.ControllerType);

            return await MethodHelper.InvokeMethod(botCommandInfo.Method, paramList, controller);
        }

        return default;
    }

    private BotCommandInfo? GetMethod(Update update)
    {
        var commands = GetMethods();
        var method = commands.FirstOrDefault(info => info.GetCustomAttributes<UpdateCommandAttribute>().Any(a => a.UpdateType == update.Type));

        if (method != null)
        {
            return new BotCommandInfo() {
                ControllerType = GetCommands().Single(x => x == method.DeclaringType),
                Method = method,
                Update = update
            };
        }

        return default;
    }

    private BotCommandInfo? GetMethod(Message message)
    {
        var methods = GetMethods();
        var method = methods.FirstOrDefault(x =>
            x.GetCustomAttributes<CommandAttribute>().Any(a => message.Text?.Equals($"/{a.Path}") ?? false) ||
            x.GetCustomAttributes<TextCommandAttribute>().Any(a => message.Text?.Equals(a.Command) ?? false) ||
            x.GetCustomAttributes<TypedCommandAttribute>().Any(a => message.Type == a.MessageType)
        );

        if (method == null)
            method = methods.SingleOrDefault(x => x.GetCustomAttributes<DefaultCommandAttribute>().Any());

        if (method != null)
        {
            return new BotCommandInfo() {
                ControllerType = GetCommands().Single(x => x == method.DeclaringType),
                Method = method,
                Message = message,
                Params = string.Join(" ", message.Text?.Split(" ").Skip(1) ?? Array.Empty<string>())
            };
        }

        return default;
    }

    private List<MethodInfo> GetMethods()
    {
        return commandCollection.CommandTypes.SelectMany(x => x.GetMethods()).ToList();
    }

    private IEnumerable<Type> GetCommands()
    {
        return commandCollection.CommandTypes;
    }
}