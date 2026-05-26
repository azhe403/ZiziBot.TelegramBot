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
    BotEngineConfig botEngineConfig
)
{
    private CommandContext _commandContext = new();

    private IEnumerable<Type> BotCommands => GetCommands();
    private List<MethodInfo> BotMethods => GetMethods();

    #region Type Handling

    public async Task<object?> HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            _commandContext.BotClient = botClient;
            _commandContext.Update = update;
            _commandContext.EngineConfig = botEngineConfig;
            
            logger.LogDebug("Session: {SessionId} - Receiving update. UpdateId: {Id}, UpdateType: {Type}, ExecutionMode: {ExecutionMode}, EngineMode: {ActualEngineMode}",
                _commandContext.SessionId, update.Id, update.Type, botEngineConfig.ExecutionMode, botEngineConfig.ActualEngineMode);   

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
            logger.LogError(e, "Session: {SessionId} - Error handling update.", _commandContext.SessionId);
            throw;
        }
    }

    private async Task TrackUpdate(CancellationToken token)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_commandContext.BotClient);
            ArgumentNullException.ThrowIfNull(_commandContext.EngineConfig);

            const int limitWarning = 5;

            if (_commandContext.EngineConfig.ActualEngineMode == BotEngineMode.Polling)
            {
                var updates = await _commandContext.BotClient.GetUpdates(cancellationToken: token);
                var updatesLength = updates.Length;
                var logLevel = updatesLength > limitWarning ? LogLevel.Warning : LogLevel.Debug;
                logger.Log(logLevel, "Session: {SessionId} - Polling Mode - GetUpdates Count: {PendingCount}", _commandContext.SessionId, updatesLength);
            }
            else
            {
                var webhookInfo = await _commandContext.BotClient.GetWebhookInfo(cancellationToken: token);
                var logLevel = webhookInfo.PendingUpdateCount > limitWarning ? LogLevel.Warning : LogLevel.Debug;
                logger.Log(logLevel, "Session: {SessionId} - Webhook Mode - Pending Updates Count: {PendingCount}", _commandContext.SessionId, webhookInfo.PendingUpdateCount);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error tracking update.", _commandContext.SessionId);
            throw;
        }
    }

    private async Task<object?> OnUpdate()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_commandContext.Update);
            ArgumentNullException.ThrowIfNull(_commandContext.BotClient);

            logger.LogTrace("Session: {SessionId} - Finding command for update type: {UpdateType}", _commandContext.SessionId, _commandContext.Update.Type);
            var method = _commandContext.Update.Type switch
            {
                UpdateType.InlineQuery => GetMethod(_commandContext.Update.InlineQuery!),
                UpdateType.CallbackQuery => GetMethod(_commandContext.Update.CallbackQuery!),
                _ => GetMethod(_commandContext.Update)
            };

            if (method == null)
            {
                logger.LogWarning("Session: {SessionId} - No handler for {UpdateType} in UpdateId: {UpdateId}", _commandContext.SessionId, _commandContext.Update.Type, _commandContext.Update.Id);
                return null;
            }

            logger.LogTrace("Session: {SessionId} - Found command for update type: {UpdateType}", _commandContext.SessionId, _commandContext.Update.Type);

            method.Update = _commandContext.Update;
            var invokeMethod = await InvokeMethod(method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Session: {SessionId} - Error on update handler.", _commandContext.SessionId);
        }

        return null;
    }

    private async Task<object?> OnMessage()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_commandContext.Update);
            ArgumentNullException.ThrowIfNull(_commandContext.BotClient);

            var message = _commandContext.Update.Message ?? _commandContext.Update.EditedMessage;
            logger.LogTrace("Session: {SessionId} - Finding command for message: {MessageText}", _commandContext.SessionId, message?.Text);
            var method = GetMethod(message!);

            if (method == null)
            {
                logger.LogWarning("Session: {SessionId} - No handler for MessageId: {MessageId} in UpdateId: {UpdateId}", _commandContext.SessionId, message?.MessageId, _commandContext.Update.Id);
                return null;
            }

            logger.LogTrace("Session: {SessionId} - Found command for message: {MessageText}", _commandContext.SessionId, message?.Text);

            method.Update = _commandContext.Update;

            var invokeMethod = await InvokeMethod(method);

            return invokeMethod;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Session: {SessionId} - Error on message handler.", _commandContext.SessionId);
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

            _commandContext = new CommandContext()
            {
                BotToken = botClientCollection.Items.First(x => x.Client == _commandContext.BotClient).BotToken,
                BotClient = _commandContext.BotClient,
                EngineConfig = botEngineConfig,
                Update = botCommandInfo.Update!
            };

            if (methodParams.Any(x => x.ParameterType == typeof(CommandContext)))
            {
                paramList =
                [
                    _commandContext
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
            logger.LogError(e, "Session: {SessionId} - Error on update invocation.", _commandContext.SessionId);
            throw;
        }
    }

    private async Task<object?> InvokeCommand(BotCommandInfo botCommandInfo, List<object> paramList)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var controller = (BotCommandController)ActivatorUtilities.CreateInstance(provider, botCommandInfo.ControllerType);
            controller.Context = _commandContext;

            logger.LogTrace("Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType}",
                _commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type);

            var invokeResult = await MethodHelper.InvokeMethod(botCommandInfo.Method, paramList, controller);

            sw.Stop();
            logger.LogDebug("Session: {SessionId} - Route: {Controller}.{Method} - UpdateId: {UpdateId} for {UpdateType} - Elapsed: {ElapsedMilliseconds}",
                _commandContext.SessionId, botCommandInfo.ControllerType.Name, botCommandInfo.Method.Name, botCommandInfo.Update!.Id, botCommandInfo.Update!.Type, sw.Elapsed);

            return invokeResult;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on command invocation.", _commandContext.SessionId);
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
                .ToList();

            var passedMiddlewareCount = 0;

            logger.LogTrace("Session: {SessionId} - BeforeMiddleware Found {Count} Middlewares", _commandContext.SessionId, beforeCommands.Count);
            foreach (var command in beforeCommands)
            {
                var middlewareName = command.GetType().Name;

                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Invoking: {Middleware}", _commandContext.SessionId, middlewareName);
                await command.ExecuteAsync(_commandContext, data =>
                {
                    passedMiddlewareCount += 1;
                    return Task.CompletedTask;
                });

                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Complete: {Middleware}", _commandContext.SessionId, middlewareName);
            }

            if (beforeCommands.Count != passedMiddlewareCount)
            {
                logger.LogDebug("Session: {SessionId} - BeforeMiddleware - Handler stops because middleware is not passed", _commandContext.SessionId);

                return false;
            }

            logger.LogTrace("Session: {SessionId} - BeforeMiddleware - All BeforeMiddlewares passed", _commandContext.SessionId);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on BeforeMiddleware invocation.", _commandContext.SessionId);
            throw;
        }
    }

    private async Task ExecuteAfterMiddlewareAsync()
    {
        try
        {
            var afterCommands = provider.GetServices<IAfterCommand>()
                .Where(x => x.GetType().GetCustomAttribute<DisabledMiddlewareAttribute>() == null)
                .ToList();

            logger.LogTrace("Session: {SessionId} - AfterMiddleware Found {Count} Middlewares", _commandContext.SessionId, afterCommands.Count);
            foreach (var command in afterCommands)
            {
                var middlewareName = command.GetType().Name;

                logger.LogDebug("Session: {SessionId} - AfterMiddleware - Invoking: {Middleware}", _commandContext.SessionId, middlewareName);
                await command.ExecuteAsync(_commandContext);
                logger.LogDebug("Session: {SessionId} - AfterMiddleware - Complete: {Middleware}", _commandContext.SessionId, middlewareName);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Session: {SessionId} - Error on AfterMiddleware invocation.", _commandContext.SessionId);
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
            logger.LogDebug("Session: {SessionId} - Fallback to default InlineQuery for InlineQueryId: {InlineQueryId}", _commandContext.SessionId, inlineQuery.Id);
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
            logger.LogDebug("Session: {SessionId} - Fallback to default CallbackQuery for CallbackQuery: {CallbackQueryId}", _commandContext.SessionId, callbackQuery.Id);
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