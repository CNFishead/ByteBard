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

        // Send the turn notification (both public and DM)
        if (result.MentionUserId.HasValue)
        {
            var combatant = tracker.Combatants.FirstOrDefault(c => c.DiscordUserId == result.MentionUserId.Value);
            var characterName = combatant?.Name ?? "Unknown Character";

            await SendTurnNotificationAsync(context, tracker, result.MentionUserId.Value, characterName,
                result.Message, "It's your turn in the combat tracker");
        }
        else
        {
            // Fallback for when there's no user to mention
            await context.Interaction.FollowupAsync(result.Message);
        }
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
        var publicMessage = $"🔁 Turn order manually updated to index `{turnIndex}`.\n{mention}, your character `{name}` is now up.";

        // Send the turn notification (both public and DM)
        if (combatant.DiscordUserId.HasValue)
        {
            await SendTurnNotificationAsync(context, tracker, combatant.DiscordUserId.Value, name,
                publicMessage, "Your turn has been manually set in the combat tracker");
        }
        else
        {
            // Fallback for when there's no user to mention
            await context.Interaction.FollowupAsync(publicMessage);
        }
    }

    public async Task ListQueue(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null || !tracker.IsActive)
        {
            await context.Interaction.FollowupAsync($"❌ No active tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        if (tracker.TurnQueue.Count == 0)
        {
            await context.Interaction.FollowupAsync("📭 No combatants remain in the queue.", ephemeral: true);
            return;
        }

        var lines = tracker.TurnQueue
            .Select((id, index) =>
            {
                var c = tracker.Combatants.FirstOrDefault(cm => cm.Id == id);
                if (c == null)
                    return $"{index + 1}. [unknown combatant {id}]";
                var name = string.IsNullOrWhiteSpace(c.Name) ? "Unknown Combatant" : c.Name;
                return $"{index + 1}. {name} (initiative {c.Initiative})";
            });

        var message = "🎯 Remaining combatants:\n" + string.Join("\n", lines);
        await context.Interaction.FollowupAsync(message);
    }

    public async Task ListOrder(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null)
        {
            await context.Interaction.FollowupAsync($"❌ No tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        if (tracker.Combatants.Count == 0)
        {
            await context.Interaction.FollowupAsync("📭 Tracker has no combatants.", ephemeral: true);
            return;
        }

        var lines = tracker.Combatants
            .OrderByDescending(c => c.Initiative)
            .Select((c, index) =>
            {
                var name = string.IsNullOrWhiteSpace(c.Name) ? "Unknown Combatant" : c.Name;
                return $"{index + 1}. {name} (initiative {c.Initiative})";
            });

        var message = "📜 Next round order:\n" + string.Join("\n", lines);
        await context.Interaction.FollowupAsync(message);
    }

    private async Task SendTurnNotificationAsync(SocketInteractionContext context, CombatTracker tracker, ulong userId, string characterName, string publicMessage, string dmNotificationReason)
    {
        // Send the public message with mention
        await context.Interaction.FollowupAsync(
            publicMessage,
            allowedMentions: new AllowedMentions
            {
                UserIds = new List<ulong> { userId }
            }
        );

        // Also send a DM to ensure the user gets notified
        try
        {
            var user = context.Client.GetUser(userId);
            if (user != null)
            {
                var dmMessage = $"🎯 **Turn Notification**\n" +
                              $"{dmNotificationReason} in **{context.Guild.Name}** #{context.Channel.Name}!\n" +
                              $"Character: **{characterName}**\n" +
                              $"Round: **{tracker.CurrentRound}**, Turn: **{tracker.CurrentTurnIndex}**";

                await user.SendMessageAsync(dmMessage);
                _logger.LogInformation($"DM sent to user {user.Username} ({user.Id}) for combat turn notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to send DM to user {userId}: {ex.Message}");
            // DM failed, but don't let it break the main functionality
        }
    }

}