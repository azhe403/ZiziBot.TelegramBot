using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.TelegramBot.Framework.Attributes;
using ZiziBot.TelegramBot.Framework.Extensions;
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
    BotEngineConfig botEngineConfig,
    CommandContext commandContext
)
{
    private IEnumerable<Type> BotCommands => commandCollection.CommandTypes;
    private IReadOnlyList<MethodInfo> BotMethods => commandCollection.Methods.Count != 0 ? commandCollection.Methods : GetMethods();

    #region Type Handling

    public async Task<object?> HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            commandContext.BotClient = botClient;
            commandContext.Update = update;
            commandContext.EngineConfig = botEngineConfig;

            logger.LogDebug("Session: {SessionId} - Receiving update. UpdateId: {Id}, UpdateType: {Type}, ExecutionMode: {ExecutionMode}, EngineMode: {ActualEngineMode}",
                commandContext.SessionId, update.Id, update.Type, botEngineConfig.ExecutionMode, botEngineConfig.ActualEngineMode);

            var result = update.Type switch
            {
                UpdateType.Message => await OnMessage(),
                UpdateType.EditedMessage => await OnMessage(),
                _ => await OnUpdate()
            };

            TrackUpdate(cancellationToken).FireAndForget(logger);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error handling update.", commandContext.SessionId);
            throw;
        }
    }

    private async Task TrackUpdate(CancellationToken token)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(commandContext.BotClient);
            ArgumentNullException.ThrowIfNull(commandContext.EngineConfig);

            const int limitWarning = 5;

            if (commandContext.EngineConfig.ActualEngineMode == BotEngineMode.Polling)
            {
                var updates = await commandContext.BotClient.GetUpdates(cancellationToken: token);
                var updatesLength = updates.Length;
                var logLevel = updatesLength > limitWarning ? LogLevel.Warning : LogLevel.Debug;
                logger.Log(logLevel, "Session: {SessionId} - Polling Mode - GetUpdates Count: {PendingCount}", commandContext.SessionId, updatesLength);
            }
            else
            {
                var webhookInfo = await commandContext.BotClient.GetWebhookInfo(cancellationToken: token);
                var logLevel = webhookInfo.PendingUpdateCount > limitWarning ? LogLevel.Warning : LogLevel.Debug;
                logger.Log(logLevel, "Session: {SessionId} - Webhook Mode - Pending Updates Count: {PendingCount}", commandContext.SessionId, webhookInfo.PendingUpdateCount);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error tracking update.", commandContext.SessionId);
            throw;
        }
    }

    private async Task<object?> OnUpdate()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(commandContext.Update);
            ArgumentNullException.ThrowIfNull(commandContext.BotClient);

            logger.LogTrace("Session: {SessionId} - Finding command for update type: {UpdateType}", commandContext.SessionId, commandContext.Update.Type);
            var method = commandContext.Update.Type switch
            {
                UpdateType.InlineQuery => GetMethod(commandContext.Update.InlineQuery!),
                UpdateType.CallbackQuery => GetMethod(commandContext.Update.CallbackQuery!),
                _ => GetMethod(commandContext.Update)
            };

            if (method == null)
            {
                logger.LogWarning("Session: {SessionId} - No handler for {UpdateType} in UpdateId: {UpdateId}", commandContext.SessionId, commandContext.Update.Type,
                    commandContext.Update.Id);
                return null;
            }

            logger.LogTrace("Session: {SessionId} - Found command for update type: {UpdateType}", commandContext.SessionId, commandContext.Update.Type);

            method.Update = commandContext.Update;
            var invokeMethod = await InvokeMethod(method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Session: {SessionId} - Error on update handler.", commandContext.SessionId);
        }

        return null;
    }

    private async Task<object?> OnMessage()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(commandContext.Update);
            ArgumentNullException.ThrowIfNull(commandContext.BotClient);

            var message = commandContext.Update.Message ?? commandContext.Update.EditedMessage;
            logger.LogTrace("Session: {SessionId} - Finding command for message: {MessageText}", commandContext.SessionId, message?.Text);
            var method = GetMethod(message!);

            if (method == null)
            {
                logger.LogWarning("Session: {SessionId} - No handler for MessageId: {MessageId} in UpdateId: {UpdateId}", commandContext.SessionId, message?.MessageId,
                    commandContext.Update.Id);
                return null;
            }

            logger.LogTrace("Session: {SessionId} - Found command for message: {MessageText}", commandContext.SessionId, message?.Text);

            method.Update = commandContext.Update;

            var invokeMethod = await InvokeMethod(method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Session: {SessionId} - Error on message handler.", commandContext.SessionId);
        }

        return null;
    }

    #endregion

    #region Invocation

    private async Task<object?> InvokeMethod(BotCommandInfo? botCommandInfo)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(botCommandInfo);

            List<object> paramList = [];
            var methodParams = botCommandInfo.Method.GetParameters();

            commandContext.SetContext(new CommandContext()
            {
                BotToken = botClientCollection.Items.First(x => x.Client == commandContext.BotClient).BotToken,
                BotClient = commandContext.BotClient,
                EngineConfig = botEngineConfig,
                Update = botCommandInfo.Update!
            });

            if (methodParams.Any(x => x.ParameterType == typeof(CommandContext)))
            {
                paramList =
                [
                    commandContext
                ];
            }

            var middlewarePassed = await ExecuteBeforeMiddlewareAsync();
            if (!middlewarePassed)
            {
                return null;
            }

            var invokeResult = await InvokeCommand(botCommandInfo, paramList);

            await ExecuteAfterMiddlewareAsync();

            return invokeResult;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on update invocation.", commandContext.SessionId);
            throw;
        }
    }

    private async Task<object?> InvokeCommand(BotCommandInfo botCommandInfo, List<object> paramList)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, botCommandInfo.ControllerType);
            controller.Context = commandContext;

            logger.LogTrace("Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType}",
                commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type);

            var invokeResult = await MethodHelper.InvokeMethod(botCommandInfo.Method, paramList, controller);

            sw.Stop();
            logger.LogDebug("Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType} - Elapsed: {ElapsedMilliseconds}",
                commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type, sw.Elapsed);

            return invokeResult;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on command invocation.", commandContext.SessionId);
            throw;
        }
    }

    private async Task<bool> ExecuteBeforeMiddlewareAsync()
    {
        try
        {
            var beforeCommands = provider.GetServices<IBeforeCommand>()
                .Where(x => x.GetType().GetCustomAttribute<DisabledMiddlewareAttribute>() == null)
                .Where(x => botEngineConfig.DisabledMiddleware?.Contains(x.GetType().Name) == false)
                .Where(x =>
                {
                    var middlewareFilter = x.GetType().GetCustomAttributes<MiddlewareFilterAttribute>();
                    var middlewareFilters = middlewareFilter as MiddlewareFilterAttribute[] ?? middlewareFilter.ToArray();
                    
                    if (middlewareFilters.Any())
                        return middlewareFilters.Any(y => y.UpdateType == commandContext.Update?.Type);

                    return true;
                })
                .ToList();

            var passedMiddlewareCount = 0;

            logger.LogTrace("Session: {SessionId} - BeforeMiddleware Found {Count} Middlewares", commandContext.SessionId, beforeCommands.Count);
            foreach (var command in beforeCommands)
            {
                var middlewareName = command.GetType().Name;

                var middlewareConfig = command as IMiddlewareConfig;
                if (middlewareConfig != null)
                {
                    middlewareConfig.Config = botEngineConfig;
                }

                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Invoking: {Middleware}", commandContext.SessionId, middlewareName);
                await command.ExecuteAsync(commandContext, data =>
                {
                    passedMiddlewareCount += 1;
                    return Task.CompletedTask;
                });

                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Complete: {Middleware}", commandContext.SessionId, middlewareName);
            }

            if (beforeCommands.Count != passedMiddlewareCount)
            {
                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Handler stops because middleware is not passed", commandContext.SessionId);

                return false;
            }

            logger.LogTrace("Session: {SessionId} - BeforeMiddleware - All BeforeMiddlewares passed", commandContext.SessionId);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on BeforeMiddleware invocation.", commandContext.SessionId);
            throw;
        }
    }

    private async Task ExecuteAfterMiddlewareAsync()
    {
        try
        {
            var afterCommands = provider.GetServices<IAfterCommand>()
                .Where(x => x.GetType().GetCustomAttribute<DisabledMiddlewareAttribute>() == null)
                .Where(x =>
                {
                    var middlewareFilter = x.GetType().GetCustomAttributes<MiddlewareFilterAttribute>();
                    var middlewareFilters = middlewareFilter as MiddlewareFilterAttribute[] ?? middlewareFilter.ToArray();
                    
                    if (middlewareFilters.Any())
                        return middlewareFilters.Any(y => y.UpdateType == commandContext.Update?.Type);

                    return true;
                })
                .ToList();

            logger.LogTrace("Session: {SessionId} - AfterMiddleware Found {Count} Middlewares", commandContext.SessionId, afterCommands.Count);
            foreach (var command in afterCommands)
            {
                var middlewareName = command.GetType().Name;

                if (command is IMiddlewareConfig middlewareConfig)
                {
                    middlewareConfig.Config = botEngineConfig;
                }

                logger.LogDebug("Session: {SessionId} - AfterMiddleware - Invoking: {Middleware}", commandContext.SessionId, middlewareName);
                await command.ExecuteAsync(commandContext);
                logger.LogDebug("Session: {SessionId} - AfterMiddleware - Complete: {Middleware}", commandContext.SessionId, middlewareName);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on AfterMiddleware invocation.", commandContext.SessionId);
            throw;
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
            logger.LogDebug("Session: {SessionId} - Fallback to default InlineQuery for InlineQueryId: {InlineQueryId}", commandContext.SessionId, inlineQuery.Id);
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
            logger.LogDebug("Session: {SessionId} - Fallback to default CallbackQuery for CallbackQuery: {CallbackQueryId}", commandContext.SessionId, callbackQuery.Id);
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
        var method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<CommandAttribute>().Any(a =>
            message.Text?.Split(" ").FirstOrDefault()?.Equals($"/{a.Path}") ?? false));

        if (method == null)
        {
            var messageText = message.Text ?? string.Empty;
            method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<TextCommandAttribute>().Any(a =>
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
            method = BotMethods.FirstOrDefault(x => x.GetCustomAttributes<TypedCommandAttribute>().Any(a => message.Type == a.MessageType));

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

    #endregion
}