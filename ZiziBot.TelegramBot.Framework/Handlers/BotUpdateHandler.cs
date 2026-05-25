using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Helpers;
using ZiziBot.TelegramBot.Framework.Interfaces;
using ZiziBot.TelegramBot.Framework.Models;
using ZiziBot.TelegramBot.Framework.Models.Configs;
using ZiziBot.TelegramBot.Framework.Models.Enums;

namespace ZiziBot.TelegramBot.Framework.Handlers;

public class BotUpdateHandler(
    IServiceProvider provider,
    ILogger<BotUpdateHandler> logger,
    BotCommandCollection commandCollection,
    BotClientCollection botClientCollection,
    BotEngineConfig botEngineConfig
)
{
    private IEnumerable<Type> BotCommands => GetCommands();
    private List<MethodInfo> BotMethods => GetMethods();

    #region Type Handling

    public async Task<object?> HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var result = update.Type switch
        {
            UpdateType.Message => await OnMessage(botClient, update),
            UpdateType.EditedMessage => await OnMessage(botClient, update),
            _ => await OnUpdate(botClient, update)
        };

        await TrackUpdate(botClient, token);

        return result;
    }

    private async Task TrackUpdate(ITelegramBotClient botClient, CancellationToken token)
    {
        const int limitWarning = 5;

        if (botEngineConfig.ActualEngineMode == BotEngineMode.Polling)
        {
            var updates = await botClient.GetUpdates(cancellationToken: token);
            var updatesLength = updates.Length;
            var logLevel = updatesLength > limitWarning ? LogLevel.Warning : LogLevel.Debug;
            logger.Log(logLevel, "Polling Mode - GetUpdates Count: {PendingCount}", updatesLength);
        }
        else
        {
            var webhookInfo = await botClient.GetWebhookInfo(cancellationToken: token);
            var logLevel = webhookInfo.PendingUpdateCount > limitWarning ? LogLevel.Warning : LogLevel.Debug;
            logger.Log(logLevel, "Webhook Mode - Pending Updates Count: {PendingCount}", webhookInfo.PendingUpdateCount);
        }
    }

    private async Task<object?> OnUpdate(ITelegramBotClient botClient, Update update)
    {
        try
        {
            logger.LogTrace("Finding command for update type: {UpdateType}", update.Type);
            var method = update.Type switch
            {
                UpdateType.InlineQuery => GetMethod(update.InlineQuery!),
                UpdateType.CallbackQuery => GetMethod(update.CallbackQuery!),
                _ => GetMethod(update)
            };

            if (method == null)
            {
                logger.LogWarning("No handler for {UpdateType} in UpdateId: {UpdateId}", update.Type, update.Id);
                return null;
            }

            logger.LogTrace("Found command for update type: {UpdateType}", update.Type);

            method.Update = update;
            var invokeMethod = await InvokeMethod(botClient, method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return null;
    }

    private async Task<object?> OnMessage(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message ?? update.EditedMessage;
            logger.LogTrace("Finding command for message: {MessageText}", message?.Text);
            var method = GetMethod(message!);

            if (method == null)
            {
                logger.LogWarning("No handler for MessageId: {MessageId} in UpdateId: {UpdateId}", message?.MessageId, update.Id);
                return null;
            }

            logger.LogTrace("Found command for message: {MessageText}", message?.Text);

            method.Update = update;

            var invokeMethod = await InvokeMethod(botClient, method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error on message handler");
        }

        return null;
    }

    #endregion

    #region Invocation

    private async Task<object?> InvokeMethod(ITelegramBotClient client, BotCommandInfo? botCommandInfo)
    {
        if (botCommandInfo is null)
            return null;

        List<object> paramList = [];
        var methodParams = botCommandInfo.Method.GetParameters();

        var commandContext = new CommandContext()
        {
            BotToken = botClientCollection.Items.First(x => x.Client == client).BotToken,
            BotClient = client,
            EngineConfig = botEngineConfig,
            Update = botCommandInfo.Update!
        };

        if (methodParams.Any(x => x.ParameterType == typeof(CommandContext)))
        {
            paramList =
            [
                commandContext
            ];
        }

        var middlewarePassed = await ExecuteBeforeMiddlewareAsync(commandContext);
        if (!middlewarePassed)
        {
            return null;
        }

        var invokeResult = await InvokeCommand(botCommandInfo, commandContext, paramList);

        await ExecuteAfterMiddlewareAsync(commandContext);

        return invokeResult;
    }

    private async Task<object?> InvokeCommand(BotCommandInfo botCommandInfo, CommandContext commandContext, List<object> paramList)
    {
        #region Invoke Command

        var sw = Stopwatch.StartNew();
        var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, botCommandInfo.ControllerType);
        controller.Context = commandContext;

        logger.LogTrace("Executing handler - Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType}",
            commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type);

        var invokeResult = await MethodHelper.InvokeMethod(botCommandInfo.Method, paramList, controller);

        sw.Stop();
        logger.LogDebug("Complete handler - Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType} - Elapsed: {ElapsedMilliseconds}",
            commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type, sw.Elapsed);

        #endregion

        return invokeResult;
    }

    private async Task<bool> ExecuteBeforeMiddlewareAsync(CommandContext commandContext)
    {
        var beforeCommands = provider.GetServices<IBeforeCommand>()
            .Where(x => x.GetType().GetCustomAttribute<DisabledMiddlewareAttribute>() == null)
            .Where(x => botEngineConfig.DisabledMiddleware?.Contains(x.GetType().Name) == false)
            .ToList();

        var passedMiddlewareCount = 0;

        logger.LogTrace("BeforeMiddleware - session: {SessionId} - Found {Count} Middlewares", commandContext.SessionId, beforeCommands.Count);
        foreach (var command in beforeCommands)
        {
            var middlewareName = command.GetType().Name;

            logger.LogDebug("BeforeMiddleware - session: {SessionId} - Invoking: {Middleware}", commandContext.SessionId, middlewareName);
            await command.ExecuteAsync(commandContext, data =>
            {
                passedMiddlewareCount += 1;
                return Task.CompletedTask;
            });

            logger.LogDebug("BeforeMiddleware - session: {SessionId} - Complete: {Middleware}", commandContext.SessionId, middlewareName);
        }

        if (beforeCommands.Count != passedMiddlewareCount)
        {
            logger.LogDebug("BeforeMiddleware - session: {SessionId} - Handler stops because middleware is not passed", commandContext.SessionId);

            return false;
        }

        logger.LogTrace("BeforeMiddleware - session: {SessionId} - All BeforeMiddlewares passed", commandContext.SessionId);
        return true;
    }

    private async Task ExecuteAfterMiddlewareAsync(CommandContext commandContext)
    {
        var afterCommands = provider.GetServices<IAfterCommand>()
            .Where(x => x.GetType().GetCustomAttribute<DisabledMiddlewareAttribute>() == null)
            .ToList();

        logger.LogTrace("AfterMiddleware - session: {SessionId} - Found {Count} Middlewares", commandContext.SessionId, afterCommands.Count);
        foreach (var command in afterCommands)
        {
            var middlewareName = command.GetType().Name;

            logger.LogDebug("AfterMiddleware - session: {SessionId} - Invoking: {Middleware}", commandContext.SessionId, middlewareName);
            await command.ExecuteAsync(commandContext);
            logger.LogDebug("AfterMiddleware - session: {SessionId} - Complete: {Middleware}", commandContext.SessionId, middlewareName);
        }
    }

    #endregion

    #region Command

    private BotCommandInfo? GetMethod(Update update)
    {
        var method = BotMethods.FirstOrDefault(info => info.GetCustomAttributes<UpdateCommandAttribute>().Any(a => a.UpdateType == update.Type));

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = BotCommands.Single(x => x == method.DeclaringType),
                Method = method,
                Update = update
            };
        }

        return null;
    }

    private BotCommandInfo? GetMethod(InlineQuery inlineQuery)
    {
        var inlineQueryCommands = inlineQuery.Query.Split(" ");
        var inlineQueryCommand = inlineQueryCommands.FirstOrDefault();
        var method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<InlineQueryAttribute>().Any(attribute => attribute.Command == inlineQueryCommand));

        if (method == null)
        {
            logger.LogDebug("Fallback to default InlineQuery for InlineQueryId: {InlineQueryId}", inlineQuery.Id);
            method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<InlineQueryAttribute>().Any());
        }

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = BotCommands.Single(x => x == method.DeclaringType),
                Method = method,
            };
        }

        return null;
    }

    private BotCommandInfo? GetMethod(CallbackQuery callbackQuery)
    {
        var callbackQueryCommands = callbackQuery.Data?.Split(" ");
        var callbackQueryCommand = callbackQueryCommands?.FirstOrDefault();
        var method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<CallbackAttribute>().Any(attribute => attribute.Command == callbackQueryCommand));

        if (method == null)
        {
            logger.LogDebug("Fallback to default CallbackQuery for CallbackQuery: {CallbackQueryId}", callbackQuery.Id);
            method = BotMethods.Where(x => x.GetCustomAttributes<CallbackAttribute>().Any())
                .FirstOrDefault(x => string.IsNullOrEmpty(x.GetCustomAttribute<CallbackAttribute>()?.Command));
        }

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = BotCommands.Single(x => x == method.DeclaringType),
                Method = method,
            };
        }

        return null;
    }

    private BotCommandInfo? GetMethod(Message message)
    {
        var method = BotMethods.Find(x => x.GetCustomAttributes<CommandAttribute>().Any(a => message.Text?.Split(" ").FirstOrDefault()?.Equals($"/{a.Path}") ?? false));

        if (method == null)
        {
            var messageText = message.Text ?? string.Empty;
            method = BotMethods.Find(x => x.GetCustomAttributes<TextCommandAttribute>().Any(a =>
            {
                return a.ComparisonMode switch
                {
                    ComparisonMode.CommandLike => messageText.Split(' ').FirstOrDefault()?.Equals(a.Command, StringComparison.OrdinalIgnoreCase) ?? false,
                    ComparisonMode.Contains => messageText.Contains(a.Command, StringComparison.OrdinalIgnoreCase),
                    _ => messageText.Equals(a.Command, StringComparison.OrdinalIgnoreCase)
                };
            }));
        }

        if (method == null)
            method = BotMethods.Find(x => x.GetCustomAttributes<TypedCommandAttribute>().Any(a => message.Type == a.MessageType));

        if (method == null)
            method = BotMethods.SingleOrDefault(x => x.GetCustomAttributes<DefaultCommandAttribute>().Any());

        if (method != null)
        {
            return new BotCommandInfo()
            {
                ControllerType = BotCommands.Single(x => x == method.DeclaringType),
                Method = method,
                Message = message,
                Params = string.Join(" ", message.Text?.Split(" ").Skip(1) ?? Array.Empty<string>())
            };
        }

        return null;
    }

    #endregion

    #region Reflection

    private List<MethodInfo> GetMethods()
    {
        return commandCollection.CommandTypes.SelectMany(x => x.GetMethods()).ToList();
    }

    private IEnumerable<Type> GetCommands()
    {
        return commandCollection.CommandTypes;
    }

    #endregion
}