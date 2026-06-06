# Engines: Polling vs Webhook

## Engine Mode Selection

The public configuration (`BotEngine:EngineMode`) is resolved into the runtime configuration (`BotEngineConfig.ActualEngineMode`) using:

- the configured mode (`Polling`, `Webhook`, `Auto`)
- the hosting environment (`Development` => Polling, otherwise Webhook)

Implementation: [ClientExtension](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L86-L100).

## Polling Engine

File: [BotPollingEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L11-L104)

What it does:

- Deletes any existing webhook: [BotPollingEngine.Start](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L21-L41)
- Starts `StartReceiving(...)` with:
  - update handler: [BotEngineHandler.UpdateHandler](../../ZiziBot.TelegramBot.Framework/Handlers/BotEngineHandler.cs#L21-L49)
  - error handler: [BotPollingEngine.ErrorHandler](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L99-L103)
- Registers running clients in [BotClientCollection](../../ZiziBot.TelegramBot.Framework/Models/BotClientCollection.cs#L5-L108)

Stop behavior:

- Cancels the per-bot `CancellationTokenSource` when removing from the collection: [BotPollingEngine.Stop](../../ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs#L71-L78)

## Webhook Engine

File: [BotWebhookEngine.cs](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L11-L103)

What it does:

- Deletes any existing webhook first.
- Requires `BotEngineConfig.WebhookUrl` to be set (otherwise it logs and returns). See [BotWebhookEngine.Start](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L34-L38).
- Builds a webhook URL that points back to the hosting ASP.NET Core server. See [BotWebhookEngine.Start](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L40-L55).
- Registers running clients in [BotClientCollection](../../ZiziBot.TelegramBot.Framework/Models/BotClientCollection.cs#L5-L108).

Stop behavior:

- Calls `DeleteWebhook()` when a bot is removed. See [BotWebhookEngine.Stop](../../ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs#L74-L80).

## Webhook HTTP Endpoints

Webhook endpoints are only mapped when `ActualEngineMode == Webhook` (see [UseZiziBotTelegramBot](../../ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs#L143-L152)).

The route prefix is:

- `api/telegram-webhook` (see [ValueConst.WebHookPath](../../ZiziBot.TelegramBot.Framework/Models/Constants/ValueConst.cs#L3-L6))

Implementation: [EndpointExtension.StartWebhookModeInternal](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L18-L143).

### Routes

- `POST /api/telegram-webhook/{bot}`
  - Works only if `BotEngineConfig.WebhookKey` is not set.
  - For backward compatibility, `{bot}` may be a bot token or a bot name.
  - Implementation: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L61-L71)
- `POST /api/telegram-webhook/{webhookKey}/{bot}`
  - `{webhookKey}` must match config; otherwise returns 404 (to avoid leaking whether a key exists).
  - `{bot}` may be token or name; the webhook engine can be configured to use the name segment to avoid exposing tokens in URLs.
  - Implementation: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L73-L108)

### Debug Route

- `GET /api/telegram-webhook/{bot}`
  - Only available when `WebhookKey` is not set.
  - Used as a discovery/help endpoint (returns a message after calling `GetMe()`).
  - Implementation: [EndpointExtension](../../ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs#L110-L141)
