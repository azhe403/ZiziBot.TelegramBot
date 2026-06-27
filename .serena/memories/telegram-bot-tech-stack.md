# Tech Stack

- Language: C# for the Telegram bot framework library and sample host.
- Framework targets .NET 8.0, 9.0, and 10.0 (multi-targeting in `ZiziBot.TelegramBot.Framework.csproj`).
- Sample host targets .NET 10.0 only.
- SDK version: `global.json` pins SDK `10.0.0` with roll-forward enabled.
- C# projects have nullable reference types and implicit usings enabled.
- Key dependencies:
  - WTelegramBot (Telegram bot client library)
  - JetBrains.Annotations (code annotations)
  - Scrutor (assembly scanning for DI)
  - UUIDNext (UUID generation)
  - Serilog.AspNetCore (logging in sample)
  - Microsoft.AspNetCore.App (framework reference)
- Sample host uses ASP.NET Core minimal hosting model.
- No test projects currently exist in the solution.
- GitHub Actions workflow for NuGet package publishing.