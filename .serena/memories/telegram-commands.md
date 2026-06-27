# Adding a New Command

To add a new command, you can create a new class that inherits from `BotCommandController` and add methods with the `[Command]` attribute. For example:

```csharp
public class SampleCommands(CommandContext context) : BotCommandController
{
    [Command("ping")]
    public async Task PingCommand()
    {
        await SendMessage("Pong!");
    }
}
```

## Command Attributes

The framework provides several attributes for defining commands:

-   `[Command("...")]`: Defines a command that is triggered by a specific string.
-   `[TextCommand("...")]`: Defines a command that is triggered by a specific text message.
-   `[Callback("...")]`: Defines a command that is triggered by a callback query.
-   `[DefaultCommand]`: Defines a command that is executed when no other command matches.
-   `[InlineQuery]`: Defines a command that is triggered by an inline query.
-   `[UpdateCommand]`: Defines a command that is triggered by a specific update type.
-   `[TypedCommand]`: Defines a command that is triggered by a specific message type.

## Command Context

The `BotCommandController` base class provides a `Context` property that allows you to access the current request and send a response. The `CommandContext` object contains information about the incoming update, such as the message, callback query, or inline query.

## Sending Responses

The `BotCommandController` base class provides several methods for sending responses:

-   `SendMessage(...)`: Sends a text message with optional reply markup.
-   `AnswerCallbackQuery(...)`: Answers a callback query.
-   `AnswerInlineQuery(...)`: Answers an inline query.
