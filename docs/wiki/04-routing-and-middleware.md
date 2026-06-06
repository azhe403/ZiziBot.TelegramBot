# Routing & Middleware

## Routing Model Summary

The framework routes each incoming update to a controller method using attributes placed on methods of `BotCommandController` subclasses.

The router implementation is in [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489).

## Message Routing (`UpdateType.Message` / `UpdateType.EditedMessage`)

Method selection happens in [GetMethod(Message)](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L425-L477) in this order:

1. **Slash command** via `[Command("...")]` (supports `/command@botname`). See [CommandAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/CommandAttribute.cs#L5-L10).
2. **Text command** via `[TextCommand("...")]` with `ComparisonMode` options (`Match`, `Contains`, `CommandLike`). See [TextCommandAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/TextCommandAttribute.cs#L6-L12).
3. **Typed message handler** via `[TypedCommand(MessageType.X)]`. See [TypedCommandAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/TypedCommandAttribute.cs#L6-L11).
4. **Fallback** via `[DefaultCommand]`. See [DefaultCommandAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/DefaultCommandAttribute.cs#L5-L8).

If the first token is a command and additional text is present, the remainder is stored into `BotCommandInfo.Params` and later exposed as `Context.CommandParam`. See [GetMethod(Message)](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L425-L473) and [CommandContext.CommandParam](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.cs#L82-L85).

## Callback Query Routing (`UpdateType.CallbackQuery`)

- Uses `[Callback("cmd")]` to match the first token of `CallbackQuery.Data`.
- If no explicit match exists, falls back to a “default callback” handler: `[Callback]` with `command=null`.

See:

- [GetMethod(CallbackQuery)](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L393-L419)
- [CallbackAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/CallbackAttribute.cs#L5-L10)
- Parsing helpers: [CommandContext.CallbackQueryCmd](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.cs#L24-L30)

## Inline Query Routing (`UpdateType.InlineQuery`)

- Uses `[InlineQuery("cmd")]` to match the first token of the inline query text.
- If no match exists, falls back to any method with `[InlineQuery]`.

See:

- [GetMethod(InlineQuery)](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L366-L391)
- [InlineQueryAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/InlineQueryAttribute.cs#L5-L10)

## Other Update Types

Use `[UpdateCommand(UpdateType.X)]` for update types that aren’t routed as message/callback/inline query.

See:

- [GetMethod(Update)](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L349-L364)
- [UpdateCommandAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/UpdateCommandAttribute.cs#L6-L11)

## Middleware Pipeline

Middleware is optional code that runs around every command invocation.

### Contracts

- Before middleware must implement [IBeforeCommand](../../ZiziBot.TelegramBot.Framework/Interfaces/IBeforeCommand.cs#L7-L11).
- After middleware must implement [IAfterCommand](../../ZiziBot.TelegramBot.Framework/Interfaces/IAfterCommand.cs#L6-L10).

### Execution Semantics

Before middleware is executed first; if a middleware does not call `next(...)`, the pipeline stops and the command is not executed.

Implementation detail:

- A middleware is considered “passed” when it calls the provided `next` delegate, which increments an internal counter. See [ExecuteBeforeMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L239-L290).

### Filtering and Disabling

Before/after middleware selection applies these rules:

- A middleware class is skipped if it has `[DisabledMiddleware]`. See [DisabledMiddlewareAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/DisabledMiddlewareAttribute.cs#L1-L5).
- A middleware class is skipped if its type name appears in `BotEngine:DisabledMiddleware`. See [BotEngineConfig.DisabledMiddleware](../../ZiziBot.TelegramBot.Framework/Models/Configs/BotEngineConfig.cs#L5-L18).
- A middleware can opt into particular `UpdateType` values by using `[MiddlewareFilter(UpdateType.X)]`. See [MiddlewareFilterAttribute](../../ZiziBot.TelegramBot.Framework/Attributes/MiddlewareFilterAttribute.cs#L6-L10).

The filtering logic lives in:

- Before middleware: [BotUpdateHandler.ExecuteBeforeMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L239-L256)
- After middleware: [BotUpdateHandler.ExecuteAfterMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L305-L318)

### Config Injection into Middleware (Optional)

Middleware can implement [IMiddlewareConfig](../../ZiziBot.TelegramBot.Framework/Interfaces/IMiddlewareConfig.cs#L5-L8) to receive `BotEngineConfig` before execution. See:

- Before middleware injection: [ExecuteBeforeMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L265-L269)
- After middleware injection: [ExecuteAfterMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L325-L329)

## Examples in Sample Project

- Message commands + default command + callbacks: [SampleCommands](../../ZiziBot.TelegramBot.Sample/Commands/SampleCommands.cs#L7-L64)
- Inline queries (default + named): [InlineQueryCommands](../../ZiziBot.TelegramBot.Sample/Commands/InlineQueryCommands.cs#L7-L39)
- Message-type and update-type handlers: [EventCommands](../../ZiziBot.TelegramBot.Sample/Commands/EventCommands.cs#L7-L20)
- Minimal pass-through before middleware: [BeforeCommandMiddleware](../../ZiziBot.TelegramBot.Sample/Middlewares/BeforeCommandMiddleware.cs#L7-L13)
- Filtered middleware example: [SampleMessageMiddleware](../../ZiziBot.TelegramBot.Sample/Middlewares/SampleMessageMiddleware.cs#L11-L19)
