# AGENTS.md

This file is for coding agents (and contributors) working in this repository. It provides the quickest path to understanding the project and making safe changes.

## What This Repo Is

- A Telegram bot framework library: `ZiziBot.TelegramBot.Framework/`
- A minimal ASP.NET Core sample host: `ZiziBot.TelegramBot.Sample/`
- Solution: `ZiziBot.TelegramBot.sln`

## Start Here (Docs)

- Code wiki entry point: `docs/wiki/01-architecture.md`
- Serena project notes: `.serena/memories/` (including `wiki.md`, `core.md`, `conventions.md`)

## Key Entry Points

- Sample app entry point: `ZiziBot.TelegramBot.Sample/Program.cs`
- Framework DI + engine selection: `ZiziBot.TelegramBot.Framework/Extensions/ClientExtension.cs`
- Update routing + middleware + invocation: `ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs`
- Engines:
  - Polling: `ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs`
  - Webhook: `ZiziBot.TelegramBot.Framework/Engines/BotWebhookEngine.cs`
- Webhook endpoints: `ZiziBot.TelegramBot.Framework/Extensions/EndpointExtension.cs`

## Build & Run

```powershell
dotnet build
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

Local URL (launch profile): `http://localhost:5157` (`ZiziBot.TelegramBot.Sample/Properties/launchSettings.json`).

## Configuration & Secrets

- Config section: `BotEngine` (see `ZiziBot.TelegramBot.Framework/Models/Configs/BotEngineConfig.cs`)
- Tokens list: `BotEngine:Bot` (see `ZiziBot.TelegramBot.Framework/Models/Configs/BotTokenConfig.cs`)

Do not commit real bot tokens, webhook keys, or other secrets. Prefer environment variables for local runs.

## Change Safety Notes

- Prefer following existing patterns (DI via `AddZiziBotTelegramBot`, routing via attributes on `BotCommandController` subclasses).
- Ensure webhook endpoints do not accidentally expose tokens/keys in logs or URLs when adding new features.
