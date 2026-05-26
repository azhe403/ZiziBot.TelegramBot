# Configuration

The bot's configuration is managed through the `appsettings.json` file. This file allows you to set up the bot's tokens, engine mode, and other settings.

### Bot Tokens

You can configure multiple bot tokens in the `BotTokenConfig` section. Each bot has a unique name and token.

```json
"BotTokenConfig": {
  "Bots": [
    {
      "Name": "Main",
      "Token": "YOUR_BOT_TOKEN",
      "Username": "YOUR_BOT_USERNAME"
    }
  ]
}
```

### Engine Mode

The `EngineMode` property determines how the bot receives updates from Telegram. The available modes are:

-   **`Polling`**: The bot periodically polls Telegram for new updates.
-   **`Webhook`**: Telegram sends updates to a specified URL.
-   **`Auto`**: The bot uses `Polling` in the `Development` environment and `Webhook` in other environments.

```json
"BotEngineConfig": {
  "EngineMode": "Auto",
  "WebhookUrl": "YOUR_WEBHOOK_URL"
}
```