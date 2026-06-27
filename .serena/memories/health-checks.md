# Health Checks Implementation

**New files**:
- `ZiziBot.TelegramBot.Framework/HealthChecks/BotConnectionHealthCheck.cs`
- `ZiziBot.TelegramBot.Framework/HealthChecks/BotWebhookHealthCheck.cs`
- `ZiziBot.TelegramBot.Framework/Extensions/HealthChecksExtension.cs`

**BotConnectionHealthCheck**:
- Tests bot connectivity by calling `GetMe()` API for each configured bot
- Returns Healthy if all bots are connected
- Returns Degraded if some bots are unhealthy
- Returns Unhealthy if no bots are configured or all fail
- Provides detailed data including healthy/unhealthy bot lists

**BotWebhookHealthCheck**:
- Automatically skips if not in webhook mode
- Monitors webhook URL configuration, pending update counts, and last errors
- Warns if pending update count exceeds 10
- Reports webhook errors with timestamps
- Provides detailed webhook information per bot
- Refactored to reduce cognitive complexity by extracting methods

**HealthChecksExtension**:
- Provides `AddZiziBotTelegramBotHealthChecks()` extension method
- Allows custom health check names via optional parameters
- Integrates with ASP.NET Core health check system

**Framework changes**:
- `BotClientCollection`: Added `Count` property and `GetAll()` method for health check access
- `ClientExtension`: Added `BotEngineConfigValidator` to DI container and validation call in startup

**Sample application changes**:
- `Program.cs`: Added health check registration and endpoints (`/health`, `/health/ready`)
- No package changes - uses built-in ASP.NET Core health checks

**Documentation**:
- New: `docs/wiki/10-health-checks.md`
- Updated: `docs/wiki/00-index.md`, `docs/wiki/06-configuration.md`, `docs/wiki/07-running.md`, `AGENTS.md`, `README.md`

**Related memories**: `mem:config-validation` for configuration validation implementation.
