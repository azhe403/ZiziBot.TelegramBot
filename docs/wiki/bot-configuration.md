# Configuration

## Configuration Entry Points

The framework binds configuration from the `BotEngine` section using:

- `BotEngineConfig.ConfigPath = "BotEngine"`: [BotEngineConfig](../../ZiziBot.TelegramBot.Framework/Models/Configs/BotEngineConfig.cs#L5-L18)
- `BotTokenConfig.ConfigPath = "BotEngine:Bot"`: [BotTokenConfig](../../ZiziBot.TelegramBot.Framework/Models/Configs/BotTokenConfig.cs#L3-L9)

Binding logic is in [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L71-L101).

## Configuration Validation

The framework automatically validates configuration during startup via `UseZiziBotTelegramBot()`. Validation checks:

- At least one bot token is configured
- Bot names are unique and non-empty
- Bot tokens match the Telegram token format (`\d+:[A-Za-z0-9_-]{35}`)
- Webhook URL is valid HTTPS when in webhook mode
- Webhook URL is provided when in webhook mode

Validation is implemented in [BotEngineConfigValidator](../../ZiziBot.TelegramBot.Framework/Validation/BotEngineConfigValidator.cs#L1-L107).

If validation fails, the application will throw an `InvalidOperationException` with detailed error messages during startup.

## `BotEngineConfig` Fields

Source: [BotEngineConfig](../../ZiziBot.TelegramBot.Framework/Models/Configs/BotEngineConfig.cs#L5-L18)

- `EngineMode`: `Auto`, `Polling`, `Webhook`
- `ActualEngineMode`: derived at startup
- `WebhookUrl`: required for webhook mode
- `WebhookKey`: optional, adds an extra segment to webhook route and causes 404 for missing/invalid keys
- `UseBotTokenInWebhookPath`: when `false`, the engine uses the configured bot name as the URL path segment to avoid exposing tokens in webhook URLs
- `ReplyMode`: controls whether `SendMessage` replies to the triggering message (see [CommandContext.Request.SendMessage](../../ZiziBot.TelegramBot.Framework/Models/Context/CommandContext.Request.cs#L36-L40))
- `ExecutionMode`: `Await` vs `Background` (see [BotEngineHandler.UpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L21-L35))
- `DisabledMiddleware`: list of middleware class names to skip (checked in [BotUpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs#L239-L246))
- `Bot`: list of `{ Name, Token }` pairs (multi-bot support)

## Sample Settings

The sample project demonstrates the configuration shape in:

- [appsettings.Development.json](../../ZiziBot.TelegramBot.Sample/appsettings.Development.json#L1-L29)
- [appsettings.json](../../ZiziBot.TelegramBot.Sample/appsettings.json#L1-L9)

## Secret Management (Bot Tokens)

Bot tokens should not be committed to source control.

Recommended approaches:

- Use environment variables (works with standard .NET configuration).
- Use per-machine configuration in your deployment environment (container secrets, CI/CD secrets, etc.).

### Environment Variable Example (PowerShell)

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:BotEngine__EngineMode="Polling"
$env:BotEngine__Bot__0__Name="Main"
$env:BotEngine__Bot__0__Token="YOUR_TELEGRAM_BOT_TOKEN"
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

### Webhook Mode Variables

Webhook mode additionally requires:

- `BotEngine__WebhookUrl` (public HTTPS base URL reachable by Telegram)
- Optional: `BotEngine__WebhookKey`
- Optional: `BotEngine__UseBotTokenInWebhookPath=false` (to keep tokens out of URLs)

The base webhook path is `api/telegram-webhook` (see [ValueConst](../../ZiziBot.TelegramBot.Framework/Models/Constants/ValueConst.cs#L3-L6)).
