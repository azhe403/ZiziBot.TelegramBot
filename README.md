# ZiziBot.TelegramBot

[![CI](https://github.com/azhe403/ZiziBot.TelegramBot/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/ZiziBot/ZiziBot.TelegramBot/actions/workflows/publish.yml)
[![ZiziBot.TelegramBot.Framework](https://img.shields.io/nuget/v/ZiziBot.TelegramBot.Framework.svg)](https://www.nuget.org/packages/ZiziBot.TelegramBot.Framework/)

**ZiziBot.TelegramBot** is a bot framework designed to help with command-based bot development. Some samples can be found in the **Sample** project.

This framework supports both Polling and Webhook modes.
You can specify the mode in `appsettings.json` using the `EngineMode` property.

### Auto Mode
By default, `EngineMode` is set to `Auto`.
This means that during local development, it runs using Polling, and after deployment, it automatically switches to Webhook mode.
Simply fill in the required configuration in `appsettings.json` to get started.

Please note that this is a very early-stage project, so there are no guarantees against breaking changes in the future.

This project is inspired by [VodemSharp/Allowed.Telegram.Bot](https://github.com/VodemSharp/Allowed.Telegram.Bot).

## Framework Architecture
All bot commands are inherited from the `BotCommandController` class. This class provides a `Context` property that allows you to access the current request and send a response. To better understand how to create a command, please refer to the sample project.

### Structure

The solution (`ZiziBot.TelegramBot.sln`) contains two main projects:

1.  **`ZiziBot.TelegramBot.Framework`**: A .NET library that provides the core framework for building command-based Telegram bots. It targets `net8.0`, `net9.0`, and `net10.0`.
2.  **`ZiziBot.TelegramBot.Sample`**: A .NET web project that serves as a sample implementation of the framework. It references the framework project and runs on `net10.0`.

### Dependencies

-   The **Framework** project uses `JetBrains.Annotations`, `Scrutor`, `UUIDNext`, and `WTelegramBot`.
-   The **Sample** project uses `Microsoft.AspNetCore.OpenApi` and `Serilog.AspNetCore`.

### Initialization

The bot is initialized in the `Program.cs` file of the sample project. Here's a breakdown of the key steps:

1.  **`AddZiziBotTelegramBot()`**: This extension method registers all the necessary services for the bot to run.
2.  **`UseZiziBotTelegramBot()`**: This extension method configures the bot and starts the engine.

### Adding a New Command

To add a new command, you can create a new class that inherits from `BotCommandController` and add methods with the `[Command]` attribute. For example:

```csharp
public class SampleCommands : BotCommandController
{
    [Command("ping")]
    public async Task Ping()
    {
        await SendMessageText("Pong!");
    }
}
```

### Build & Run

-   **Build**: The entire solution can be built using the `dotnet build` command from the root directory.
-   **Run**: The sample application can be run with the command `dotnet run` from the `ZiziBot.TelegramBot.Sample` directory.
-   **URL**: When running, the sample application is accessible at `http://localhost:5157`.

## Demo project

1. [ZiziBot-Engine](https://github.com/azhe403/ZiziBot-Engine)

## Thanks

Thanks to JetBrains for providing us with <a href="https://www.jetbrains.com/?from=zizibot" target="_blank">dotUltimate</a> licenses.

<a href="https://www.jetbrains.com/?from=zizibot" target="_blank">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains logo." width="200">
</a>