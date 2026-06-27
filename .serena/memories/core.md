# Core

- This is a Telegram bot framework library with two main projects: `ZiziBot.TelegramBot.Framework/` (library) and `ZiziBot.TelegramBot.Sample/` (sample ASP.NET Core host)
- The canonical solution file is `ZiziBot.TelegramBot.slnx` (legacy `.sln` still exists but should not be used)
- Framework provides command-based bot programming model with polling and webhook engine modes
- For framework-specific details, read `mem:framework-core`
- For sample implementation details, read `mem:sample-core`
- For command patterns, read `mem:telegram-commands`
- For verification steps, read `mem:verification-checklist`
- For new features (config validation, health checks, stop engine, throttled update tracking), read `mem:features`
- Human-facing architecture docs live in `docs/wiki/*.md`; source files are the source of truth if docs drift
- Build commands: `dotnet build ZiziBot.TelegramBot.slnx` and `dotnet run --project .\ZiziBot.TelegramBot.Sample`