# Framework Core (Key Types)

This page describes the key types that make up the runtime behavior of the framework.

## Composition Root

### `ClientExtension`

File: [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L17-L165)

Key responsibilities:

- **Command discovery cache**: finds all loaded `BotCommandController` subclasses and caches their methods in `BotCommandCollection`. See [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L24-L58).
- **Middleware discovery**: uses Scrutor to scan assemblies and register `IBeforeCommand` / `IAfterCommand` implementations as scoped services. See [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L60-L68).
- **Configuration binding**: binds `BotEngineConfig` and `List<BotTokenConfig>` from configuration (or uses the supplied `engineConfig`). See [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L70-L123).
- **Engine selection**: registers both engines and resolves `IBotEngine` based on `BotEngineConfig.ActualEngineMode`. See [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L125-L133).

## Engines

### `IBotEngine`

File: [IBotEngine.cs](../../ZiziBot.TelegramBot.Framework/Interfaces/IBotEngine.cs#L5-L12)

Defines the minimal API used by the host to start and stop the bot engine, including `StopEngine()`.

### `BotPollingEngine`

File: [BotPollingEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L11-L104)

- Creates `TelegramBotClient` instances from configured tokens.
- Calls `StartReceiving(...)` and wires updates into [BotEngineHandler.UpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L21-L49).
- Stores created clients in [BotClientCollection](../../ZiziBot.TelegramBot.Framework/Models/BotClientCollection.cs#L5-L108).
- Implements `StopEngine()` to gracefully cancel all long-polling tasks using the active cancellation tokens.

### `BotWebhookEngine`

File: [BotWebhookEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L11-L103)

- Requires `BotEngine:WebhookUrl`.
- Builds webhook URLs using [ValueConst.WebHookPath](../../ZiziBot.TelegramBot.Framework/Models/Constants/ValueConst.cs#L3-L6) and optional `WebhookKey`.
- Supports hiding the bot token in the URL by using the bot name segment when `UseBotTokenInWebhookPath=false`. See [BotWebhookEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L47-L50).
- Implements `StopEngine()` to safely delete registered webhooks from Telegram API on application shutdown.

## Update Handling Pipeline

### `BotEngineHandler`

File: [BotEngineHandler.cs](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L11-L49)

Responsibilities:

- Creates a fresh DI scope for each update.
- Resolves [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489).
- Executes per `ExecutionMode`:
  - `Await`: await completion.
  - `Background`: run fire-and-forget (logging exceptions).

### `BotUpdateHandler`

File: [BotUpdateHandler.cs](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489)

Core responsibilities:

- Sets up the current [CommandContext](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.cs#L9-L120) for the update. See [BotUpdateHandler.HandleUpdate](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L35-L62).
- Routes message updates (`Message`, `EditedMessage`) vs other update types. See [BotUpdateHandler.HandleUpdate](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L46-L52).
- Resolves a target method using routing attributes, builds a [BotCommandInfo](../../ZiziBot.TelegramBot.Framework/Models/BotCommandInfo.cs#L1-L13), then invokes it.
- Runs middleware around invocation:
  - Before middleware: [ExecuteBeforeMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L239-L296)
  - After middleware: [ExecuteAfterMiddlewareAsync](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L301-L340)

## Command Programming Model

### `BotCommandController`

File: [BotCommandController.cs](../../ZiziBot.TelegramBot.Framework/Models/BotCommandController.cs#L7-L41)

- Base class for user-defined command controllers.
- Framework sets `Context` before invoking your method. See [BotUpdateHandler.InvokeCommand](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L210-L227).
- Provides convenience wrappers for sending messages and answering callbacks/inline queries.

### `CommandContext`

Files:

- [CommandContext.cs](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.cs#L9-L120)
- [CommandContext.Request.cs](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.Request.cs#L10-L93)

Contains:

- Typed accessors for user/chat/message/callback/inline query fields derived from the current `Update`.
- Session metadata (`SessionId`, `RequestDate`) for logging/correlation.
- Telegram API helper methods (SendMessage, AnswerInlineQuery, AnswerCallbackQuery), which can honor `ReplyMode` via `BotEngineConfig.ReplyMode`. See [CommandContext.Request.SendMessage](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.Request.cs#L12-L56).
