 # Task Completion

 For typical code changes:
 - `dotnet build -c Release` at repo root.
 - If changes touch runtime behavior, run sample:
   - set valid tokens in `ZiziBot.TelegramBot.Sample/appsettings.Development.json`
   - `cd ZiziBot.TelegramBot.Sample && dotnet run`
 - No linters/formatters/test runners configured beyond `dotnet build` (no test projects in repo).