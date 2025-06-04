public class CombatTracker
{
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string GameId { get; set; } = string.Empty; // friendly name, unique within channel
    public ulong CreatedByUserId { get; set; }

    public bool IsActive { get; set; } = false;
    public int CurrentTurnIndex { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Combatant> Combatants { get; set; } = new();
    public List<int> TurnQueue { get; set; } = new(); // Combatant IDs
    public int CurrentRound { get; set; } = 1;
}

public class Combatant
{
    public int Id { get; set; }
    public int TrackerId { get; set; }
    public CombatTracker Tracker { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int Initiative { get; set; }
    public ulong? DiscordUserId { get; set; } // Null = NPC
}
