
# Project Structure

The solution (`ZiziBot.TelegramBot.sln`) contains two main projects:

1.  **`ZiziBot.TelegramBot.Framework`**: A .NET library that provides the core framework for building command-based Telegram bots. It targets `net8.0`, `net9.0`, and `net10.0`.
2.  **`ZiziBot.TelegramBot.Sample`**: A .NET web project that serves as a sample implementation of the framework. It references the framework project and runs on `net10.0`.

## Framework Project

The `ZiziBot.TelegramBot.Framework` project contains the following key components:

-   **`Attributes`**: Custom attributes used to define commands and their properties.
-   **`Delegates`**: Delegate types used for command execution.
-   **`Engines`**: The core polling and webhook engines for receiving updates from Telegram.
-   **`Extensions`**: Extension methods for client and endpoint setup.
-   **`Handlers`**: Handlers for processing bot updates.
-   **`Helpers`**: Helper classes for various tasks.
-   **`Interfaces`**: Interfaces for middleware and other components.
-   **`Models`**: Data models for configuration, context, and other objects.

## Sample Project

The `ZiziBot.TelegramBot.Sample` project demonstrates how to use the framework and includes the following:

-   **`Commands`**: Sample command classes that inherit from `BotCommandController`.
-   **`Middlewares`**: Sample middleware implementations.
-   **`Program.cs`**: The main entry point of the application, where the bot is initialized and configured.
-   **`appsettings.json`**: Configuration file for the bot, including bot tokens and other settings.