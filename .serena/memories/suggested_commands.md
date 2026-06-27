# Suggested Commands

- Repo root restore/build: `dotnet restore ZiziBot.TelegramBot.slnx` then `dotnet build ZiziBot.TelegramBot.slnx -c Debug`.
- Run sample host from repo root: `dotnet run --project ZiziBot.TelegramBot.Sample/ZiziBot.TelegramBot.Sample.csproj`.
- Build framework only: `dotnet build ZiziBot.TelegramBot.Framework/ZiziBot.TelegramBot.Framework.csproj`.
- Create NuGet package: `dotnet pack ZiziBot.TelegramBot.Framework/ZiziBot.TelegramBot.Framework.csproj`.
- Useful Windows shell equivalents when a command must be run outside agent tools:
  - list matching files: `Get-ChildItem -Recurse -Filter *.csproj`
  - search file content: `Select-String -Path **\*.cs -Pattern "SomePattern"`
  - remove a file/tree: `Remove-Item <path> -Recurse -Force`