Sample module: `ZiziBot.TelegramBot.Sample/` (ASP.NET Core minimal host).

Startup:
- `Program.cs` wires logging (Serilog), then calls `AddZiziBotTelegramBot()` and `UseZiziBotTelegramBot()`.
- Exposes `GET /` => `OK!`.

Examples:
- Commands in `Commands/` show:
  - Message commands via `[Command]` and `[TextCommand]`.
  - Default command via `[DefaultCommand]`.
  - Inline queries via `[InlineQuery]`.
  - Callback queries via `[Callback]`.
  - Non-message events via `[TypedCommand]` and `[UpdateCommand]`.
- Middleware in `Middlewares/` show `IBeforeCommand`/`IAfterCommand` and disabling via `[DisabledMiddleware]` and `BotEngine.DisabledMiddleware`.

Config:
- `appsettings.Development.json` contains `BotEngine` section with `EngineMode`, `WebhookUrl`, `DisabledMiddleware`, and bot tokens (`Bot`).
- Align `ReplyStrategy`/`ExecutionStrategy` keys to `ReplyMode`/`ExecutionMode` if you want binding to work automatically.