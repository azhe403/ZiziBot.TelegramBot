# New Features

This memory lists the major features added to the framework recently and outlines their design.

## 1. Dynamic Configuration Validation
- **Validator**: `BotEngineConfigValidator` (in `ZiziBot.TelegramBot.Framework/Validation/`)
- **Key aspects**:
  - Automatically validates bound configuration at startup under `BotEngine`.
  - Uses C# Source-Generated Regular Expressions for the bot token format checks to optimize compilation and startup latency.
  - Ensures uniquely named bots and valid HTTPS webhook URLs when webhook mode is active.

## 2. Graceful Shutdown & Engine Stopping
- **Key aspects**:
  - Implemented the `StopEngine` / `StopAsync` hooks across polling (`BotPollingEngine`) and webhook (`BotWebhookEngine`) engines.
  - Engines cancel their internal `CancellationTokenSource` contexts on stop.
  - Webhook engine deletes its webhook registration on shutdown to ensure clean Telegram bot states.
  - Connected directly to the host's lifetime hooks in `ClientExtension`.

## 3. Throttled Update Tracking
- **Key aspects**:
  - Optimized update tracking loops.
  - The framework's background tracking for active updates (via `TrackUpdate` in update handler pipeline) is throttled to run at a 5-second interval, significantly reducing unnecessary telemetry/network spam.

## 4. Diagnostics & Exception Logging
- **Key aspects**:
  - Catch `ArgumentException` in `BotPollingEngine` when the Telegram bot client detects an invalid token.
  - Log the full stack trace and exception details instead of a simple error message string, facilitating fast developer troubleshooting.

---

**See Also**:
- `mem:health-checks` for details on Readiness/Liveness probes.
- `mem:config-validation` for validation rules.
