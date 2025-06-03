using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FallVerseBotV2.Commands.Combat;
using Microsoft.EntityFrameworkCore;

public class TurnOrderHandler
{
    private readonly ILogger<TurnOrderHandler> _logger;
    private readonly BotDbContext _db;

    public TurnOrderHandler(ILogger<TurnOrderHandler> logger, BotDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Advance(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);
        if (tracker == null)
        {
            await context.Interaction.FollowupAsync("❌ No active tracker found.", ephemeral: true);
            return;
        }

        var result = TurnProcessor.Advance(tracker);
        await _db.SaveChangesAsync();

        if (result == null)
        {
            await context.Interaction.FollowupAsync("⚠️ No valid combatant found.", ephemeral: true);
            return;
        }

        SocketGuildUser? user = null;

        if (result.MentionUserId.HasValue)
        {
            user = context.Guild.GetUser(result.MentionUserId.Value);
        }

        string userMention = user?.Mention ?? $"<@{result.MentionUserId}>";

        await context.Interaction.FollowupAsync(
            result.Message.Replace($"<@{result.MentionUserId}>", userMention),
            allowedMentions: new AllowedMentions
            {
                UserIds = result.MentionUserId.HasValue ? new List<ulong> { result.MentionUserId.Value } : new List<ulong>()
            }
        );
    }
    public async Task SetTurn(SocketInteractionContext context, string gameId, int turnIndex)
    {
        await context.Interaction.DeferAsync();

        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;
        var userId = context.User.Id;

        // STEP 1: Load the tracker
        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null || !tracker.IsActive)
        {
            await context.Interaction.FollowupAsync($"❌ No active tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        // STEP 2: Permission check
        if (!TrackerUtils.IsTrackerOwnerOrAdmin(tracker, context))
        {
            await context.Interaction.FollowupAsync("⛔ Only the tracker creator or server admins can manually set the turn.", ephemeral: true);
            return;
        }

        // STEP 3: Validate turn index
        if (turnIndex < 0 || turnIndex >= tracker.TurnQueue.Count)
        {
            await context.Interaction.FollowupAsync($"⚠️ Invalid turn index `{turnIndex}`. There are only `{tracker.TurnQueue.Count}` entries in the queue.", ephemeral: true);
            return;
        }

        // STEP 4: Update the turn index
        tracker.CurrentTurnIndex = turnIndex;

        // Get the new combatant
        int combatantId = tracker.TurnQueue[turnIndex];
        var combatant = tracker.Combatants.FirstOrDefault(c => c.Id == combatantId);

        if (combatant == null)
        {
            await context.Interaction.FollowupAsync("❌ Combatant not found. Queue may be desynced.", ephemeral: true);
            return;
        }

        await _db.SaveChangesAsync();

        var mention = $"<@{combatant.DiscordUserId}>";
        var name = string.IsNullOrWhiteSpace(combatant.Name) ? "Unknown Combatant" : combatant.Name;

        await context.Interaction.FollowupAsync($"🔁 Turn order manually updated to index `{turnIndex}`.\n{mention}, your character `{name}` is now up.");
    }

}