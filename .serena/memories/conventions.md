# Conventions

- Follow existing framework patterns: DI via `AddZiziBotTelegramBot()`, routing via attributes on `BotCommandController` subclasses
- Keep extension methods focused: configuration validation and health checks are implemented as separate services
- Use constructor injection with primary constructor syntax where appropriate
- Avoid using fully qualified namespaces in code files when a using directive can be used to shorten it (keep code clean and concise)
- Maintain cognitive complexity below thresholds by extracting complex methods into smaller, focused functions
- Repo-level formatting defaults: LF line endings, UTF-8, trimmed trailing whitespace, file-scoped namespaces in C#, 4-space indentation for `.cs`, 2-space indentation for JSON
- ReSharper settings prefer explicit object creation types when the type is not obvious
- Use `.slnx` for solution-level work (canonical solution file). The legacy `.sln` still exists but should not be used for new work.
- Do not commit real bot tokens, webhook keys, or other secrets to the repository
- Prefer environment variables for local configuration
- Add XML documentation comments to public APIs in the framework library
- Update AGENTS.md and wiki documentation when adding new framework features
