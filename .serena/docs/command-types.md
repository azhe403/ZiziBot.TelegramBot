
# Command Types

The framework supports several types of commands, which are decorated with specific attributes.

#### Text Commands

Text commands are triggered by sending a message that starts with a forward slash (`/`) or a specific text.

-   **`[Command("command")]`**: This attribute is used to define a command that is triggered by `/command`.
-   **`[TextCommand("text")]`**: This attribute is used to define a command that is triggered by the exact text `text`.
-   **`[DefaultCommand]`**: This attribute is used to define a command that is triggered when no other command matches.

```csharp
public class SampleCommands : BotCommandController
{
    [Command("ping")]
    [TextCommand("ping")]
    public async Task PingCommand()
    {
        await SendMessage("Pong!");
    }

    [Command("say")]
    public async Task SayCommand()
    {
        await SendMessage($"You say: {Context.CommandParam}!");
    }

    [DefaultCommand]
    public async Task DefaultCommand()
    {
        await SendMessage("Default Command!");
    }
}
```

#### Callback Queries

Callback queries are triggered when a user clicks an inline keyboard button.

-   **`[Callback("data")]`**: This attribute is used to define a command that is triggered when the callback data is `data`.
-   **`[Callback]`**: This attribute is used to define a command that is triggered when no other callback query matches.

```csharp
public class SampleCommands : BotCommandController
{
    [Callback("ping")]
    public async Task PingCallback()
    {
        await AnswerCallbackQuery("User clicked Ping!");
    }

    [Callback]
    public async Task DefaultCallback()
    {
        var text = $"Cmd: {Context.CallbackQueryCmd}" +
                   $"\nParam: {Context.CallbackQueryParam}";
        await AnswerCallbackQuery(text, showAlert: true);
    }
}
```

#### Inline Queries

Inline queries are triggered when a user types the bot's username followed by a query in any chat.

-   **`[InlineQuery("query")]`**: This attribute is used to define a command that is triggered when the inline query is `query`.
-   **`[InlineQuery]`**: This attribute is used to define a command that is triggered when no other inline query matches.

```csharp
public class InlineQueryCommands : BotCommandController
{
    [InlineQuery]
    public async Task InlineQueryCommand()
    {
        await AnswerInlineQuery(new List<InlineQueryResult>() {
            new InlineQueryResultArticle("default-inline", "Default Inline", new InputTextMessageContent("Default Inline Content")),
            new InlineQueryResultArticle("hello-inline", "Hello Inline", new InputTextMessageContent("Hello Inline Content"))
        });
    }

    [InlineQuery("hello")]
    public async Task InlineQueryCommandHello()
    {
        await AnswerInlineQuery(new List<InlineQueryResult>() {
            new InlineQueryResultArticle("aa576dec-0727-4ea1-99ae-3c7cb20ea3c8", "Hello Inline", new InputTextMessageContent("Hello Inline Content"))
        });
    }
}
```

#### Event-Based Commands

Event-based commands are triggered by specific events in a chat.

-   **`[TypedCommand(MessageType.NewChatMembers)]`**: This attribute is used to define a command that is triggered when a new member joins the chat.
-   **`[UpdateCommand(UpdateType.ChatJoinRequest)]`**: This attribute is used to define a command that is triggered when a user requests to join a chat.

```csharp
public class EventCommands : BotCommandController
{
    [TypedCommand(MessageType.NewChatMembers)]
    public async Task NewChatMembersCommand()
    {
        await SendMessage("Halo!");
    }

    [UpdateCommand(UpdateType.ChatJoinRequest)]
    public async Task ChatJoinRequestCommand()
    {
        await SendMessage("Chat join request!");
    }
}
```