using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FallVerseBotV2.Commands.Combat;
using Microsoft.EntityFrameworkCore;

public class TrackerLifecycleHandler
{
    private readonly ILogger<TrackerLifecycleHandler> _logger;
    private readonly BotDbContext _db;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _services;

    public TrackerLifecycleHandler(
    ILogger<TrackerLifecycleHandler> logger,
    ILoggerFactory loggerFactory,
    BotDbContext db,
    IServiceProvider services)
    {
        _logger = logger;
        _db = db;
        _loggerFactory = loggerFactory;
        _services = services;
    }

    public TrackerLifecycleHandler(ILogger<TrackerLifecycleHandler> logger, BotDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task CreateTracker(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();
        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;
        var creatorId = context.User.Id;

        // Ensure uniqueness of GameId within channel
        var exists = await _db.CombatTrackers
          .AnyAsync(t => t.GuildId == guildId && t.ChannelId == channelId && t.GameId == gameId);

        if (exists)
        {
            await context.Interaction.FollowupAsync($"❌ A tracker with the game ID `{gameId}` already exists in this channel.", ephemeral: true);
            return;
        }

        var tracker = new CombatTracker
        {
            GuildId = guildId,
            ChannelId = channelId,
            GameId = gameId,
            CreatedByUserId = creatorId,
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            CurrentTurnIndex = 0,
        };

        _db.CombatTrackers.Add(tracker);
        await _db.SaveChangesAsync();

        await context.Interaction.FollowupAsync($"✅ Combat tracker `{gameId}` created in this channel.");
    }

    public async Task StartTracker(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);
        if (tracker == null || tracker.IsActive || !tracker.Combatants.Any())
        {
            await context.Interaction.FollowupAsync("❌ Could not start tracker (not found, already active, or no combatants).", ephemeral: true);
            return;
        }

        tracker.TurnQueue = tracker.Combatants
            .OrderByDescending(c => c.Initiative)
            .Select(c => c.Id)
            .ToList();

        tracker.IsActive = true;
        tracker.CurrentRound = 1;
        tracker.CurrentTurnIndex = 0;

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


    public async Task Reset(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;

        // STEP 1: Load the tracker and its combatants
        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null)
        {
            await context.Interaction.FollowupAsync($"❌ No tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        // STEP 2: Validate combatants exist
        if (!tracker.Combatants.Any())
        {
            await context.Interaction.FollowupAsync($"⚠️ Tracker `{gameId}` has no combatants to reset.", ephemeral: true);
            return;
        }

        // STEP 3: Sort combatants again by initiative (in case changed)
        tracker.Combatants = tracker.Combatants
          .OrderByDescending(c => c.Initiative)
          .ToList();

        tracker.CurrentTurnIndex = 0;
        tracker.CurrentRound = 1;
        tracker.IsActive = true;

        await _db.SaveChangesAsync();

        // STEP 4: Announce first turn again
        var first = tracker.Combatants.First();

        string userMention = $"<@{first.DiscordUserId}>";
        string charName = string.IsNullOrWhiteSpace(first.Name) ? "Unknown Combatant" : first.Name;

        var resetMessage = $"🔄 Tracker `{gameId}` has been reset.\n{userMention}, your character `{charName}` is up.";

        await context.Interaction.FollowupAsync(resetMessage);
    }

    public async Task Delete(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        // STEP 1: Load tracker with combatants
        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null)
        {
            await context.Interaction.FollowupAsync($"❌ No tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        // STEP 2: Check ownership or admin rights
        if (!TrackerUtils.IsTrackerOwnerOrAdmin(tracker, context))
        {
            await context.Interaction.FollowupAsync("⛔ You do not have permission to delete this tracker.", ephemeral: true);
            return;
        }

        // STEP 3: Remove the tracker and its combatants
        _db.CombatTrackers.Remove(tracker);
        await _db.SaveChangesAsync();

        await context.Interaction.FollowupAsync($"🗑 Tracker `{gameId}` has been deleted.");
    }

    public async Task Advance(SocketInteractionContext context, string gameId)
    {
        await context.Interaction.DeferAsync();

        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;

        var tracker = await TrackerUtils.TryGetTrackerAsync(_db, context, gameId);

        if (tracker == null || !tracker.IsActive)
        {
            await context.Interaction.FollowupAsync($"❌ No active tracker with ID `{gameId}` found in this channel.", ephemeral: true);
            return;
        }

        if (tracker.TurnQueue.Count == 0)
        {
            // Regenerate queue at the start of a new round
            tracker.CurrentRound++;
            var ordered = tracker.Combatants
                .OrderByDescending(c => c.Initiative)
                .Select(c => c.Id)
                .ToList();

            tracker.TurnQueue = ordered;
            tracker.CurrentTurnIndex = 0;
        }

        // Pop the next combatant ID from the queue
        var nextId = tracker.TurnQueue.First();
        tracker.TurnQueue.RemoveAt(0);
        tracker.CurrentTurnIndex++;

        // Fetch the combatant by ID
        var combatant = tracker.Combatants.FirstOrDefault(c => c.Id == nextId);

        if (combatant == null)
        {
            await context.Interaction.FollowupAsync($"⚠️ Next combatant not found. Turn skipped.", ephemeral: true);
            return;
        }

        var mention = $"<@{combatant.DiscordUserId}>";
        var name = string.IsNullOrWhiteSpace(combatant.Name) ? "Unknown Combatant" : combatant.Name;
        var message = $"🔁 Round {tracker.CurrentRound}, Turn {tracker.CurrentTurnIndex}\n{mention}, your character `{name}` is up.";

        await _db.SaveChangesAsync();
        await context.Interaction.FollowupAsync(message);
    }
}