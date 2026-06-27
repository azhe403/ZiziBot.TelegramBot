# Running the Project

## Prerequisites

- .NET SDK capable of building:
  - Framework: `net8.0`, `net9.0`, `net10.0` ([Framework.csproj](../../ZiziBot.TelegramBot.Framework/ZiziBot.TelegramBot.Framework.csproj#L1-L32))
  - Sample host: `net10.0` ([Sample.csproj](../../ZiziBot.TelegramBot.Sample/ZiziBot.TelegramBot.Sample.csproj#L1-L18))

## Build

From the repository root:

```powershell
dotnet build ZiziBot.TelegramBot.slnx
```

Reference: [README.md](../../README.md#L19-L23).

## Run the Sample Host (Recommended)

```powershell
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

The launch profile defines the default local URL as:

- `http://localhost:5157` (see [launchSettings.json](../../ZiziBot.TelegramBot.Sample/Properties/launchSettings.json#L12-L21))

The sample app’s minimal endpoint returns `"OK!"`:

- `GET /` (see [Program.cs](../../ZiziBot.TelegramBot.Sample/Program.cs#L14-L19))

## Configure Bot Tokens

The bot will not function until at least one bot token is configured under `BotEngine:Bot`.

Options:

- Set environment variables (recommended): see [06-configuration.md](./06-configuration.md)
- Put local-only values in `ZiziBot.TelegramBot.Sample/appsettings.Development.json` (ensure it stays uncommitted)

## Choose Polling vs Webhook

### Polling (best for local development)

- Set `BotEngine:EngineMode` to `Polling`, or leave `Auto` and run in `Development`.
- Implementation of auto-selection: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L90-L100).

### Webhook (for deployment)

Webhook requires:

- A public HTTPS URL for your running host (`BotEngine:WebhookUrl`)
- Inbound reachability by Telegram
- Optional `BotEngine:WebhookKey` if you want a keyed route segment

Webhook endpoints are under `api/telegram-webhook`:

- Constant: [ValueConst](../../ZiziBot.TelegramBot.Framework/Models/Constants/ValueConst.cs#L3-L6)
- Mapping: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L18-L143)

## Smoke-Test with Sample Commands

The sample project includes command controllers demonstrating the routing model:

- `/ping`, `/start`, `/say`, default command, callbacks: [SampleCommands](../../ZiziBot.TelegramBot.Sample/Commands/SampleCommands.cs#L7-L64)
- Inline query handlers: [InlineQueryCommands](../../ZiziBot.TelegramBot.Sample/Commands/InlineQueryCommands.cs#L7-L39)
- Message/update type handlers: [EventCommands](../../ZiziBot.TelegramBot.Sample/Commands/EventCommands.cs#L7-L20)

## Health Checks

The sample project includes health check endpoints for monitoring:

- `/health` - All health checks (bot connection + webhook)
- `/health/ready` - Readiness probe (bot connection only)

See [10-health-checks.md](./10-health-checks.md) for detailed health check documentation.
