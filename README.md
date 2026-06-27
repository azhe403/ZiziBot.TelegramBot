# ZiziBot.TelegramBot

[![CI](https://github.com/azhe403/ZiziBot.TelegramBot/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/azhe403/ZiziBot.TelegramBot/actions/workflows/publish.yml)
[![ZiziBot.TelegramBot.Framework](https://img.shields.io/nuget/v/ZiziBot.TelegramBot.Framework.svg)](https://www.nuget.org/packages/ZiziBot.TelegramBot.Framework/)

A command-based, middleware-driven Telegram bot framework for .NET 8/9/10, featuring a scoped execution pipeline, robust startup configuration validation, complete liveness/readiness health probes, and graceful engine lifecycle hooks.

Includes a ready-to-run minimal sample host in ASP.NET Core.

---

## 🚀 Key Features

- **Dual-Engine Update Pipeline**: Seamless execution under **Long Polling** mode (ideal for local development) or **Webhooks** (ideal for production environments), with automatic routing fallback.
- **Attribute-Based Routing**: Clean controller methods decorated with `[Command]`, `[TextCommand]`, `[TypedCommand]`, `[Callback]`, `[InlineQuery]`, and `[UpdateCommand]` to route incoming Telegram payloads.
- **Middleware Pipeline**: Support for custom before/after pipeline execution hooks (`IBeforeCommand`, `IAfterCommand`) executing within a scoped Dependency Injection (DI) lifecycle.
- **Dynamic Configuration Validation**: Automatic, regex-based validation of bot tokens and engine parameters at startup to prevent invalid deployments.
- **Health Checks & Diagnostics**: Built-in endpoints for `/health` (full diagnostics) and `/health/ready` (connectivity tests using `GetMe()` calls) to integrate with orchestrators.
- **Graceful Shutdown**: Native lifecycle hooks that handle cancellation and clean teardown of both polling and webhook engines.
- **Throttled Diagnostics**: Efficient update logging and diagnostics throttling to prevent logs flooding during active traffic.
- **Robust Exception Logging**: Captured and logged full exception stack traces (such as on client library token rejection) for better troubleshooting.

---

## 📂 Project Structure

- **`ZiziBot.TelegramBot.Framework/`**: The main reusable library containing:
  - **Engines**: `BotPollingEngine` and `BotWebhookEngine` transport implementations.
  - **Handlers**: The pipeline boundary, update router, and middleware executor.
  - **Validation**: Regex-based bot engine settings validation.
  - **HealthChecks**: Probe engines checking Telegram connectivity and webhooks.
- **`ZiziBot.TelegramBot.Sample/`**: A sample ASP.NET Core host showcasing:
  - Command routing examples (text, callbacks, inline queries).
  - Custom before/after middleware pipeline wireup.
  - Dependency Injection setup in `Program.cs`.

---

## 🛠️ Quick Start

### 1. Prerequisites
- **.NET 8.0, 9.0, or 10.0 SDK**
- A Telegram bot token (from [@BotFather](https://t.me/BotFather))

### 2. Configuration
Configure the framework under the `BotEngine` section in your application settings (e.g., `appsettings.Development.json`):

```json
{
  "BotEngine": {
    "EngineMode": "Auto",
    "ReplyMode": "ReplyToSender",
    "ExecutionMode": "Background",
    "Bot": [
      {
        "Name": "MainBot",
        "Token": "YOUR_TELEGRAM_BOT_TOKEN"
      }
    ]
  }
}
```
*Note: Do not commit real tokens to source control. Prefer environment variables for secrets.*

### 3. Build & Run
```powershell
# Build solution
dotnet build ZiziBot.TelegramBot.slnx

# Run the sample host
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

The sample app runs locally on `http://localhost:5157`.

### 4. Health Checks
Monitor the application's runtime status using the built-in probes:
- **Liveness probe**: `http://localhost:5157/health`
- **Readiness probe** (checks active bot connection): `http://localhost:5157/health/ready`

---

## 📖 Documentation & Guides

For deep-dive topics, check out the following resources:
- 📖 **[Code Wiki Index](./docs/wiki/00-index.md)** - Main documentation index.
- 🏗️ **[Telegram Bot Architecture](./docs/wiki/01-telegram-bot-architecture.md)** - High-level and sequence diagrams.
- 🚦 **[Routing & Middleware](./docs/wiki/04-routing-and-middleware.md)** - Writing controllers and interceptors.
- ⚙️ **[Configuration Validation](./docs/wiki/06-bot-configuration.md)** - Validator options and patterns.
- 🏥 **[Health Checks reference](./docs/wiki/10-health-checks.md)** - Diagnostic probes configuration.
- 👥 **[Contributor & Agent Guide](./AGENTS.md)** - Safe development practices.

---

## 💖 Shoutout

Thanks to JetBrains for providing us with <a href="https://www.jetbrains.com/?from=zizibot" target="_blank">dotUltimate</a> licenses.

<a href="https://www.jetbrains.com/?from=zizibot" target="_blank">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains logo." width="200">
</a>