# ZiziBot.TelegramBot

[![CI](https://github.com/azhe403/ZiziBot.TelegramBot/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/ZiziBot/ZiziBot.TelegramBot/actions/workflows/publish.yml)
[![ZiziBot.TelegramBot.Framework](https://img.shields.io/nuget/v/ZiziBot.TelegramBot.Framework.svg)](https://www.nuget.org/packages/ZiziBot.TelegramBot.Framework/)

Command-based Telegram bot framework + sample host.

## Docs

- Contributor/agent guide: [AGENTS.md](./AGENTS.md)
- Code wiki (start here): [docs/wiki/00-index.md](./docs/wiki/00-index.md)
- Health checks: [docs/wiki/10-health-checks.md](./docs/wiki/10-health-checks.md)

## Projects

- Framework (library): `ZiziBot.TelegramBot.Framework/`
- Sample host (ASP.NET Core): `ZiziBot.TelegramBot.Sample/`

## Quick Start

```powershell
dotnet build ZiziBot.TelegramBot.slnx
dotnet run --project .\ZiziBot.TelegramBot.Sample
```

Configure at least one bot token via `BotEngine:Bot` (prefer environment variables; do not commit real tokens).

## Shoutout

Thanks to JetBrains for providing us with <a href="https://www.jetbrains.com/?from=zizibot" target="_blank">dotUltimate</a> licenses.

<a href="https://www.jetbrains.com/?from=zizibot" target="_blank">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains logo." width="200">
</a>