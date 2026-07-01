
# Project Structure

The solution (`ZiziBot.TelegramBot.slnx`) contains two main projects:

1.  **`ZiziBot.TelegramBot.Framework`**: A .NET library that provides the core framework for building command-based Telegram bots. It targets `net8.0`, `net9.0`, and `net10.0`.
2.  **`ZiziBot.TelegramBot.Sample`**: A .NET web project that serves as a sample implementation of the framework. It references the framework project and runs on `net10.0`.