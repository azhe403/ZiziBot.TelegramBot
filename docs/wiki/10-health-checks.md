# Health Checks

## Overview

The framework provides built-in health checks for monitoring bot status and webhook health. These health checks integrate with ASP.NET Core's health check system and can be used with standard monitoring tools.

## Health Check Types

### Bot Connection Health Check

Checks the connectivity status of all configured bot clients:

- Verifies each bot can connect to Telegram API
- Tests by calling `GetMe()` for each bot
- Returns Healthy if all bots are connected
- Returns Degraded if some bots are unhealthy
- Returns Unhealthy if no bots are configured or all fail

Implementation: [BotConnectionHealthCheck](../../ZiziBot.TelegramBot.Framework/HealthChecks/BotConnectionHealthCheck.cs#L1-L65)

### Bot Webhook Health Check

Monitors webhook status when in webhook mode:

- Skips automatically if not in webhook mode
- Checks webhook URL is configured for each bot
- Monitors pending update count (warns if > 10)
- Reports last webhook errors
- Returns detailed webhook information in health check data

Implementation: [BotWebhookHealthCheck](../../ZiziBot.TelegramBot.Framework/HealthChecks/BotWebhookHealthCheck.cs#L1-L98)

## Usage

### Adding Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddZiziBotTelegramBotHealthChecks();
```

### Custom Health Check Names

You can customize the health check names:

```csharp
builder.Services.AddHealthChecks()
    .AddZiziBotTelegramBotHealthChecks(
        botConnectionName: "my-bot-connection",
        webhookName: "my-bot-webhook"
    );
```

### Mapping Health Check Endpoints

```csharp
// All health checks
app.MapHealthChecks("/health");

// Only bot connection health (for readiness probes)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "bot-connection"
});
```

## Health Check Response Format

### Healthy Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "bot-connection": {
      "status": "Healthy",
      "data": {
        "HealthyBots": ["Main (@mybot)", "Secondary (@mybot2)"]
      }
    },
    "bot-webhook": {
      "status": "Healthy",
      "data": {
        "WebhookInfo": [
          {
            "BotName": "Main",
            "Url": "https://example.com/api/telegram-webhook/...",
            "PendingUpdateCount": 0,
            "LastError": "None",
            "MaxConnections": 40
          }
        ]
      }
    }
  }
}
```

### Degraded Response

```json
{
  "status": "Degraded",
  "totalDuration": "00:00:00.2345678",
  "entries": {
    "bot-connection": {
      "status": "Degraded",
      "description": "Some bots are unhealthy: Secondary",
      "data": {
        "HealthyBots": ["Main (@mybot)"],
        "UnhealthyBots": ["Secondary"]
      }
    }
  }
}
```

## Production Deployment

### Kubernetes Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 10
```

### Kubernetes Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 30
```

## Monitoring

Health checks can be integrated with:

- Azure Monitor Health Checks
- Application Insights
- Prometheus exporters
- Custom monitoring dashboards

The health check data includes detailed information about bot status, pending updates, and webhook configuration for advanced monitoring scenarios.
