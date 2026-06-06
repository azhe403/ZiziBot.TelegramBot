# Code Wiki Index

This repository contains:

- A Telegram bot framework library: [`ZiziBot.TelegramBot.Framework/`](../../ZiziBot.TelegramBot.Framework)
- A minimal ASP.NET Core sample host: [`ZiziBot.TelegramBot.Sample/`](../../ZiziBot.TelegramBot.Sample)

Use this index page as the entry point for understanding the codebase and runtime flow.

## Wiki Pages

- Architecture overview: [01-architecture.md](./01-architecture.md)
- Project structure & module responsibilities: [02-project-structure.md](./02-project-structure.md)
- Framework core (key types): [03-framework-core.md](./03-framework-core.md)
- Routing & middleware: [04-routing-and-middleware.md](./04-routing-and-middleware.md)
- Engines (webhook vs polling): [05-engines-webhook-polling.md](./05-engines-webhook-polling.md)
- Configuration: [06-configuration.md](./06-configuration.md)
- Running the project: [07-running.md](./07-running.md)
- Dependency map: [08-dependency-map.md](./08-dependency-map.md)

## Executive Summary

### Runtime Flow

At runtime the system follows this flow:

1. The host registers the framework via `AddZiziBotTelegramBot()`.
2. The host starts the framework via `UseZiziBotTelegramBot()`.
3. An engine (Polling or Webhook) receives Telegram updates and forwards them to a shared update handler.
4. Each update is processed in a fresh DI scope: routed → middleware executed → controller method invoked.

Key entry points:

- Sample host entry point: [Program.cs](../../ZiziBot.TelegramBot.Sample/Program.cs#L1-L19)
- DI + engine selection + startup: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L17-L165)
- Update routing + middleware + invocation: [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489)
- Polling engine: [BotPollingEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L11-L104)
- Webhook engine: [BotWebhookEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L11-L103)
- Webhook endpoints: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L18-L143)

### Major Modules (Framework)

- `Extensions/`: Composition root (DI wiring, config binding, engine selection) + webhook endpoint mapping.
- `Engines/`: “Ingress adapters” for Telegram updates (polling receiver vs webhook registration).
- `Handlers/`: Update processing pipeline (per-update DI scope, routing, middleware, invocation).
- `Attributes/`: Declarative routing and middleware selection attributes.
- `Models/`: Context, config models, command info, and multi-bot runtime state.
- `Interfaces/`: Engine and middleware contracts.

## Key Classes & Methods (Quick Reference)

### Hosting Integration

- `ClientExtension.AddZiziBotTelegramBot(IServiceCollection, IConfiguration?, BotEngineConfig?)`
  - Registers framework services, binds config, discovers controllers/middlewares, and selects an `IBotEngine`.
  - Implementation: [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L22-L141)
- `ClientExtension.UseZiziBotTelegramBot(WebApplication)`
  - Maps webhook endpoints (webhook mode only) and starts the selected engine.
  - Implementation: [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L143-L164)

### Update Pipeline

- `BotEngineHandler.UpdateHandler(ITelegramBotClient, Update, CancellationToken)`
  - Creates a new DI scope per update and dispatches into `BotUpdateHandler`.
  - Execution policy: await vs fire-and-forget controlled by `BotEngineConfig.ExecutionMode`.
  - Implementation: [BotEngineHandler.cs](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L21-L49)
- `BotUpdateHandler.HandleUpdate(ITelegramBotClient, Update, BotEngineConfig)`
  - Sets `CommandContext` for the update and routes to the correct method based on `UpdateType` and attributes.
  - Implementation: [BotUpdateHandler.cs](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L35-L62)

### Engines

- `BotPollingEngine.Start()`
  - Deletes any existing webhook and starts Telegram long polling (`StartReceiving`).
  - Implementation: [BotPollingEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L48-L69)
- `BotWebhookEngine.Start()`
  - Requires `BotEngine:WebhookUrl` and configures Telegram webhooks for each bot token.
  - Implementation: [BotWebhookEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L63-L72)

### Webhook Endpoint Mapping

- `EndpointExtension.StartWebhookModeInternal(WebApplication)`
  - Maps POST routes under `api/telegram-webhook` and forwards updates to `BotEngineHandler.UpdateHandler`.
  - Implementation: [EndpointExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L18-L143)

## Dependency Relationships (Layering)

The high-level dependency direction is:

- Host → Extensions → Engines → Handlers → Controllers/Models
- Handlers also depend on middleware interfaces and attribute metadata

Reference diagram: [08-dependency-map.md](./08-dependency-map.md)

## Running the Project

See the detailed guide: [07-running.md](./07-running.md).

Quick start:

```powershell
dotnet build
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

The bot requires at least one token under `BotEngine:Bot`. Prefer environment variables; do not commit real tokens.

