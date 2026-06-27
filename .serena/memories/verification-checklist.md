# Task Completion

- Minimum verification is `dotnet build ZiziBot.TelegramBot.slnx -c Debug` from repo root.
- If a change touches framework logic, also verify the sample can start: `dotnet run --project ZiziBot.TelegramBot.Sample/ZiziBot.TelegramBot.Sample.csproj`. Skip only when secrets or external config make runtime verification impractical.
- If a task changes project structure, names, or run instructions, update `docs/wiki/*.md` and `AGENTS.md` so the documentation stays aligned with the live layout.
- When adding new framework features, update `AGENTS.md`, relevant wiki pages, and add/update memory files in `.serena/memories/`.