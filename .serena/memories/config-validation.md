# Configuration Validation

**File**: `ZiziBot.TelegramBot.Framework/Validation/BotEngineConfigValidator.cs`

**Purpose**: Validates bot configuration at startup to prevent runtime failures due to misconfiguration.

**Validation rules**:
- At least one bot token must be configured under `BotEngine:Bot`
- Bot names must be unique and non-empty
- Bot tokens must match Telegram format: `\d+:[A-Za-z0-9_-]{35}`
- When in webhook mode, webhook URL must be provided and use HTTPS

**Integration**: 
- Registered as singleton in `ClientExtension.AddZiziBotTelegramBot()`
- Called automatically in `ClientExtension.UseZiziBotTelegramBot()` before engine startup
- Throws `InvalidOperationException` with detailed error messages on validation failure

**Code quality**: Removed unused static constructor and field for cleaner implementation.

**See also**: `mem:health-checks` for related health check implementation.
