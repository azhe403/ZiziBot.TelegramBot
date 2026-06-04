using Telegram.Bot;

namespace ZiziBot.TelegramBot.Framework.Models;

public class BotClientCollection
{
    private readonly object gate = new();

    public List<BotClientItem> Items { get; } = [];

    /// <summary>
    /// Checks whether a bot with the given <paramref name="name"/> is already registered.
    /// </summary>
    public bool ContainsName(string name)
    {
        lock (gate)
        {
            return Items.Exists(x => x.Name == name);
        }
    }

    /// <summary>
    /// Adds <paramref name="item"/> if there is no existing bot with the same name.
    /// </summary>
    /// <returns>True if added; false if a bot with the same name already exists.</returns>
    public bool TryAdd(BotClientItem item)
    {
        lock (gate)
        {
            if (Items.Exists(x => x.Name == item.Name))
            {
                return false;
            }

            Items.Add(item);
            return true;
        }
    }

    /// <summary>
    /// Removes a bot by name.
    /// </summary>
    /// <returns>True if removed; false if no bot exists with that name.</returns>
    public bool TryRemoveByName(string name, out BotClientItem? item)
    {
        lock (gate)
        {
            item = Items.Find(x => x.Name == name);
            if (item == null)
            {
                return false;
            }

            Items.Remove(item);
            return true;
        }
    }

    /// <summary>
    /// Gets a bot client item by bot token.
    /// </summary>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetByToken(string botToken, out BotClientItem? item)
    {
        lock (gate)
        {
            item = Items.FirstOrDefault(x => x.BotToken == botToken);
            return item != null;
        }
    }

    /// <summary>
    /// Gets a bot client item by bot name.
    /// </summary>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetByName(string name, out BotClientItem? item)
    {
        lock (gate)
        {
            item = Items.FirstOrDefault(x => x.Name == name);
            return item != null;
        }
    }

    /// <summary>
    /// Gets a bot client item by the <see cref="ITelegramBotClient"/> instance reference.
    /// </summary>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetByClient(ITelegramBotClient botClient, out BotClientItem? item)
    {
        lock (gate)
        {
            item = Items.FirstOrDefault(x => x.Client == botClient);
            return item != null;
        }
    }

    /// <summary>
    /// Returns a snapshot of registered bot names safe for iteration.
    /// </summary>
    public IReadOnlyList<string> GetNamesSnapshot()
    {
        lock (gate)
        {
            return Items.Select(x => x.Name).ToList();
        }
    }
}
