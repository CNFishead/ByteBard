using Discord.Interactions;
using Discord.WebSocket;
using FallVerseBotV2.Commands.Combat;
using Microsoft.EntityFrameworkCore;

public class CombatantHandler
{
    private readonly ILogger<CombatantHandler> _logger;
    private readonly BotDbContext _db;

    public CombatantHandler(ILogger<CombatantHandler> logger, BotDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task AddCombatant(SocketInteractionContext context, string gameId, int initiative, string? name = null)
    {
        await context.Interaction.DeferAsync();

        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;
        var userId = context.User.Id;

        // STEP 1: Load the tracker
        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null)
        {
            await context.Interaction.FollowupAsync($"❌ No tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        // STEP 2: Resolve name fallback
        var finalName = string.IsNullOrWhiteSpace(name) ? context.User.Username : name;

        // Optional: Prevent duplicate names if needed
        var nameExists = tracker.Combatants.Any(c => c.Name.Equals(finalName, StringComparison.OrdinalIgnoreCase));
        if (nameExists)
        {
            await context.Interaction.FollowupAsync($"⚠️ A combatant named `{finalName}` already exists in this tracker.", ephemeral: true);
            return;
        }

        // STEP 3: Add new combatant
        var combatant = new Combatant
        {
            Name = finalName,
            Initiative = initiative,
            DiscordUserId = userId,
            TrackerId = tracker.Id
        };

        _db.Combatants.Add(combatant);
        await _db.SaveChangesAsync();

        // STEP 4: Respond differently based on tracker state
        string response = tracker.IsActive
            ? $"⚠️ `{finalName}` added to tracker `{gameId}`.\nThey will enter combat at the start of the next round (Round {tracker.CurrentRound + 1})."
            : $"✅ Added `{finalName}` (initiative `{initiative}`) to tracker `{gameId}`.";

        await context.Interaction.FollowupAsync(response);
    }
    public async Task RemoveCombatant(SocketInteractionContext context, string gameId, string combatantName)
    {
        await context.Interaction.DeferAsync();

        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;
        var userId = context.User.Id;

        // STEP 1: Load the tracker
        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null)
        {
            await context.Interaction.FollowupAsync($"❌ No tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        // STEP 2: Validate permissions (WM or admin)
        if (!TrackerUtils.IsTrackerOwnerOrAdmin(tracker, context))
        {
            await context.Interaction.FollowupAsync("⛔ You do not have permission to remove combatants from this tracker.", ephemeral: true);
            return;
        }

        // STEP 3: Find the combatant (case-insensitive)
        var combatant = tracker.Combatants
            .FirstOrDefault(c => c.Name.Equals(combatantName, StringComparison.OrdinalIgnoreCase));

        if (combatant == null)
        {
            await context.Interaction.FollowupAsync($"⚠️ No combatant named `{combatantName}` found in tracker `{gameId}`.", ephemeral: true);
            return;
        }

        // STEP 4: Remove combatant from TurnQueue if present
        tracker.TurnQueue.RemoveAll(id => id == combatant.Id);

        // STEP 5: Remove from DB
        _db.Combatants.Remove(combatant);
        await _db.SaveChangesAsync();

        await context.Interaction.FollowupAsync($"🗑 Combatant `{combatant.Name}` removed from tracker `{gameId}`.");
    }

}