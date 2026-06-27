Framework module: `ZiziBot.TelegramBot.Framework/`.

Entry points:
- DI registration + engine selection: `Extensions/ClientExtension.AddZiziBotTelegramBot()`.
- Host startup: `Extensions/ClientExtension.UseZiziBotTelegramBot()` (includes configuration validation).
- Webhook mapping: `Extensions/EndpointExtension.StartWebhookModeInternal()` maps `/api/telegram-webhook/{botToken}`.
- Health checks: `Extensions/HealthChecksExtension.AddZiziBotTelegramBotHealthChecks()`.

Core flow:
- `IBotEngine` implementation produces updates:
  - Polling: `Engines/BotPollingEngine` uses `StartReceiving`.
  - Webhook: `Engines/BotWebhookEngine` calls `SetWebhook` to `{WebhookUrl}/api/telegram-webhook/{botToken}`.
- Updates are processed via `Handlers/BotEngineHandler` which creates an async DI scope per update and delegates to `Handlers/BotUpdateHandler`.
- `BotEngineConfig.ExecutionMode` controls await vs fire-and-forget update handling.

Routing + pipeline:
- Router: `Handlers/BotUpdateHandler` matches methods by attributes and invokes them.
- Before/after middleware resolved from DI (`IBeforeCommand`, `IAfterCommand`).
- Context: `Models/CommandContext` provides strongly-typed accessors + Telegram response helpers.

Configuration validation:
- `Validation/BotEngineConfigValidator` validates bot tokens, webhook config, and engine mode at startup.
- Throws `InvalidOperationException` with detailed error messages on validation failure.

Health checks:
- `HealthChecks/BotConnectionHealthCheck` tests bot connectivity via `GetMe()` API calls.
- `HealthChecks/BotWebhookHealthCheck` monitors webhook status, pending updates, and errors.

Known limitation:
- `BotWebhookEngine.Stop(...)` is not implemented (throws).