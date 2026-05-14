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

## Demo project

1. [ZiziBot-Engine](https://github.com/azhe403/ZiziBot-Engine)

## Thanks

Thanks to JetBrains for providing us with <a href="https://www.jetbrains.com/?from=zizibot" target="_blank">dotUltimate</a> licenses.

<a href="https://www.jetbrains.com/?from=zizibot" target="_blank">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains logo." width="200">
</a>