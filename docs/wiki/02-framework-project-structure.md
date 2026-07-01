# Project Structure & Module Responsibilities

## Solution Layout

The solution contains two projects (library + sample app):

- [ZiziBot.TelegramBot.slnx](../../ZiziBot.TelegramBot.slnx)
- Framework library: [ZiziBot.TelegramBot.Framework.csproj](../../ZiziBot.TelegramBot.Framework/ZiziBot.TelegramBot.Framework.csproj)
- Sample host: [ZiziBot.TelegramBot.Sample.csproj](../../ZiziBot.TelegramBot.Sample/ZiziBot.TelegramBot.Sample.csproj)

## Repository Tree (Conceptual)

- `ZiziBot.TelegramBot.Framework/`
  - `Extensions/` — ASP.NET Core integration, DI wiring, endpoint mapping
  - `Engines/` — polling/webhook engine implementations
  - `Handlers/` — update processing pipeline (routing + middleware + invocation)
  - `Attributes/` — declarative routing for commands and middleware filtering
  - `Interfaces/` — middleware contracts and engine interface
  - `Models/` — context objects, config models, in-memory bot registry
  - `Helpers/` — reflection helper utilities
- `ZiziBot.TelegramBot.Sample/`
  - `Program.cs` — host entry point
  - `Commands/` — example controllers demonstrating routing attributes
  - `Middlewares/` — example before/after middleware
  - `appsettings*.json` — example configuration (including bot tokens)

## Responsibility Map

### Hosting & Composition Root

- **Registers everything** and **discovers commands/middlewares** across all loaded assemblies: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L17-L141)
- **Starts the engine** and conditionally maps webhook endpoints: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L143-L164)

### Engines (How Updates Enter)

- Polling: delete webhook → `StartReceiving(...)`: [BotPollingEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L11-L69)
- Webhook: delete webhook → build URL → `SetWebhook(...)`: [BotWebhookEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L11-L72)

### Update Pipeline (Routing & Invocation)

- Per-update scope and execution policy: [BotEngineHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L11-L49)
- Routing by `UpdateType` and by attributes (command/callback/inline query): [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489)

### Command Model

- Controller base class used by user code: [BotCommandController](../../ZiziBot.TelegramBot.Framework/Models/BotCommandController.cs#L7-L41)
- Per-request context with convenience properties: [CommandContext](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.cs#L9-L120)
- Telegram API wrappers (SendMessage/AnswerInlineQuery/AnswerCallbackQuery): [CommandContext.Request](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.Request.cs#L10-L93)

### Middleware

- Before middleware: [IBeforeCommand](../../ZiziBot.TelegramBot.Framework/Interfaces/IBeforeCommand.cs#L7-L11)
- After middleware: [IAfterCommand](../../ZiziBot.TelegramBot.Framework/Interfaces/IAfterCommand.cs#L6-L10)
- Optional config injection for middleware: [IMiddlewareConfig](../../ZiziBot.TelegramBot.Framework/Interfaces/IMiddlewareConfig.cs#L5-L8)

### In-Memory Runtime State (Multi-Bot)

- Thread-safe in-memory registry of running bot clients: [BotClientCollection](../../ZiziBot.TelegramBot.Framework/Models/BotClientCollection.cs#L5-L108)
- Bot client record (name/token/client/CTS): [BotClientItem](../../ZiziBot.TelegramBot.Framework/Models/BotClientItem.cs#L5-L21)
