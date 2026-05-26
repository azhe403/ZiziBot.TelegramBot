# Middleware

Middleware is a powerful feature that allows you to execute code before or after a command is executed. This is useful for tasks such as authentication, logging, and error handling.

### Before-Command Middleware

Before-command middleware is executed before a command is executed. To create a before-command middleware, you need to create a class that implements the `IBeforeCommand` interface.

```csharp
public class UserPreparationMiddleware : IBeforeCommand
{
    public async Task ExecuteAsync(CommandContext commandContext, CommandDelegate next)
    {
        // Your logic here
        await next(commandContext);
    }
}
```

### After-Command Middleware

After-command middleware is executed after a command is executed. To create an after-command middleware, you need to create a class that implements the `IAfterCommand` interface.

```csharp
public class AfterCommandMiddleware : IAfterCommand
{
    public async Task ExecuteAsync(CommandContext commandContext, CommandDelegate next)
    {
        // Your logic here
        await next(commandContext);
    }
}
```

### Disabling Middleware

You can disable middleware for a specific command or an entire controller by using the `[DisabledMiddleware]` attribute.

```csharp
[DisabledMiddleware(typeof(AfterCommandMiddleware))]
public class SampleCommands : BotCommandController
{
    // ...
}
```