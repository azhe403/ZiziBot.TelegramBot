# Code Wiki Index

This repository contains:

- A Telegram bot framework library: [`ZiziBot.TelegramBot.Framework/`](../../ZiziBot.TelegramBot.Framework)
- A minimal ASP.NET Core sample host: [`ZiziBot.TelegramBot.Sample/`](../../ZiziBot.TelegramBot.Sample)

Use this index page as the entry point for understanding the codebase and runtime flow.

## Wiki Pages

- Architecture overview: [telegram-bot-architecture.md](./telegram-bot-architecture.md)
- Project structure & module responsibilities: [framework-project-structure.md](./framework-project-structure.md)
- Framework core (key types): [03-framework-core.md](./03-framework-core.md)
- Routing & middleware: [04-routing-and-middleware.md](./04-routing-and-middleware.md)
- Engines (webhook vs polling): [05-engines-webhook-polling.md](./05-engines-webhook-polling.md)
- Configuration: [bot-configuration.md](./bot-configuration.md)
- Running the project: [running-the-sample.md](./running-the-sample.md)
- Dependency map: [08-dependency-map.md](./08-dependency-map.md)
- Consolidated reference: [09-comprehensive-code-wiki.md](./09-comprehensive-code-wiki.md)
- Health checks: [10-health-checks.md](./10-health-checks.md)

## Executive Summary

### Runtime Flow

At runtime the system follows this flow:

1. The host registers the framework via `AddZiziBotTelegramBot()`.
2. The host starts the framework via `UseZiziBotTelegramBot()`.
3. An engine (Polling or Webhook) receives Telegram updates and forwards them to a shared update handler.
4. Each update is processed in a fresh DI scope: routed → middleware executed → controller method invoked.

Key entry points:

- Sample host entry point: [Program.cs](../../ZiziBot.TelegramBot.Sample/Program.cs#L1-L31) (includes health check registration)
- DI + engine selection + startup: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L17-L165)
- Update routing + middleware + invocation: [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L18-L489)
- Polling engine: [BotPollingEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L11-L104)
- Webhook engine: [BotWebhookEngine](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L11-L103)
- Webhook endpoints: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L18-L143)

### Major Modules (Framework)

- `Extensions/`: Composition root (DI wiring, config binding, engine selection) + webhook endpoint mapping + health checks.
- `Engines/`: “Ingress adapters” for Telegram updates (polling receiver vs webhook registration).
- `Handlers/`: Update processing pipeline (per-update DI scope, routing, middleware, invocation).
- `Attributes/`: Declarative routing and middleware selection attributes.
- `Models/`: Context, config models, command info, and multi-bot runtime state.
- `Interfaces/`: Engine and middleware contracts.
- `Validation/`: Configuration validation services.
- `HealthChecks/`: Health check implementations for bot connection and webhook status.

## Key Classes & Methods (Quick Reference)

### Hosting Integration

- `ClientExtension.AddZiziBotTelegramBot(IServiceCollection, IConfiguration?, BotEngineConfig?)`
  - Registers framework services, binds config, discovers controllers/middlewares, and selects an `IBotEngine`.
  - Also registers configuration validation service.
  - Implementation: [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L23-L143)
- `ClientExtension.UseZiziBotTelegramBot(WebApplication)`
  - Validates configuration, maps webhook endpoints (webhook mode only), and starts the selected engine.
  - Implementation: [ClientExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L145-L159)

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

### Configuration Validation

- `BotEngineConfigValidator.Validate(BotEngineConfig)`
  - Validates bot tokens, webhook configuration, and engine mode settings at startup.
  - Throws `InvalidOperationException` with detailed error messages on validation failure.
  - Implementation: [BotEngineConfigValidator.cs](../../ZiziBot.TelegramBot.Framework/Validation/BotEngineConfigValidator.cs#L1-L107)

### Health Checks

- `HealthChecksExtension.AddZiziBotTelegramBotHealthChecks(IHealthChecksBuilder, string?, string?)`
  - Registers bot connection and webhook health checks.
  - Implementation: [HealthChecksExtension.cs](../../ZiziBot.TelegramBot.Framework/Extensions/HealthChecksExtension.cs#L1-L25)
- `BotConnectionHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)`
  - Tests bot connectivity via `GetMe()` API calls.
  - Implementation: [BotConnectionHealthCheck.cs](../../ZiziBot.TelegramBot.Framework/HealthChecks/BotConnectionHealthCheck.cs#L1-L65)
- `BotWebhookHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)`
  - Monitors webhook status, pending updates, and errors.
  - Implementation: [BotWebhookHealthCheck.cs](../../ZiziBot.TelegramBot.Framework/HealthChecks/BotWebhookHealthCheck.cs#L1-L159)

## Dependency Relationships (Layering)

The high-level dependency direction is:

- Host → Extensions → Engines → Handlers → Controllers/Models
- Handlers also depend on middleware interfaces and attribute metadata

Reference diagram: [08-dependency-map.md](./08-dependency-map.md)

## Running the Project

See the detailed guide: [running-the-sample.md](./running-the-sample.md).

Quick start:

```powershell
dotnet build ZiziBot.TelegramBot.slnx
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

The bot requires at least one token under `BotEngine:Bot`. Prefer environment variables; do not commit real tokens.
