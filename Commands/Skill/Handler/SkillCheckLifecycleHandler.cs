using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class SkillCheckLifecycleHandler
{
  private readonly ILogger<SkillCheckLifecycleHandler> _logger;
  private readonly BotDbContext _db;

  public SkillCheckLifecycleHandler(ILogger<SkillCheckLifecycleHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  /// <summary>
  /// Creates a new skill check for the current channel
  /// </summary>
  public async Task InitSkillCheck(SocketInteractionContext context, string skillName, int dc, string? successMessage = null, string? failureMessage = null, int? durationMinutes = null, bool isPrivate = false)
  {
    try
    {
      await context.Interaction.DeferAsync();

      var guildId = context.Guild?.Id ?? throw new InvalidOperationException("This command can only be used in a server.");
      var channelId = context.Channel.Id;
      var userId = context.User.Id;

      // Check if there's already an active skill check for this skill in this channel
      var existingSkillCheck = await _db.SkillChecks
          .Where(sc => sc.GuildId == guildId &&
                     sc.ChannelId == channelId &&
                     sc.SkillName.ToLower() == skillName.ToLower() &&
                     sc.IsActive)
          .FirstOrDefaultAsync();

      if (existingSkillCheck != null)
      {
        await context.Interaction.FollowupAsync(
            $"⚠️ There is already an active **{skillName}** skill check in this channel. Use `/skill remove-skillcheck {skillName}` to remove it first.",
            ephemeral: true);
        return;
      }

      var skillCheck = new SkillCheck
      {
        GuildId = guildId,
        ChannelId = channelId,
        SkillName = skillName,
        DC = dc,
        SuccessMessage = successMessage,
        FailureMessage = failureMessage,
        CreatedByUserId = userId,
        ExpiresAt = durationMinutes.HasValue ? DateTime.UtcNow.AddMinutes(durationMinutes.Value) : null,
        IsPrivate = isPrivate
      };

      _db.SkillChecks.Add(skillCheck);
      await _db.SaveChangesAsync();

      var embed = new EmbedBuilder()
          .WithTitle($"🎲 {skillName} Skill Check Initialized")
          .WithColor(Color.Blue)
          .WithTimestamp(DateTimeOffset.UtcNow)
          .WithFooter($"Created by {context.User.Username}")
          .AddField("How to Roll", $"Use `/skill roll {skillName} [your_roll_result]` to attempt this skill check", false);

      if (isPrivate)
      {
        embed.AddField("Privacy Mode", "🔒 Results will be sent privately via DM", false);
      }

      if (skillCheck.ExpiresAt.HasValue)
        embed.AddField("Expires", $"<t:{((DateTimeOffset)skillCheck.ExpiresAt.Value).ToUnixTimeSeconds()}:R>", false);

      await context.Interaction.FollowupAsync(embed: embed.Build());

      _logger.LogInformation($"Skill check '{skillName}' (DC {dc}) created in guild {guildId}, channel {channelId} by user {userId}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating skill check");
      await context.Interaction.FollowupAsync("❌ An error occurred while creating the skill check.", ephemeral: true);
    }
  }

  /// <summary>
  /// Removes an active skill check
  /// </summary>
  public async Task RemoveSkillCheck(SocketInteractionContext context, string skillName)
  {
    try
    {
      await context.Interaction.DeferAsync();

      var guildId = context.Guild?.Id ?? throw new InvalidOperationException("This command can only be used in a server.");
      var channelId = context.Channel.Id;

      var skillCheck = await _db.SkillChecks
          .Include(sc => sc.Attempts)
          .Where(sc => sc.GuildId == guildId &&
                     sc.ChannelId == channelId &&
                     sc.SkillName.ToLower() == skillName.ToLower() &&
                     sc.IsActive)
          .FirstOrDefaultAsync();

      if (skillCheck == null)
      {
        await context.Interaction.FollowupAsync(
            $"❌ No active **{skillName}** skill check found in this channel.",
            ephemeral: true);
        return;
      }

      skillCheck.IsActive = false;
      await _db.SaveChangesAsync();

      var embed = new EmbedBuilder()
          .WithTitle($"🗑️ {skillName} Skill Check Removed")
          .WithDescription($"The **{skillName}** skill check has been deactivated.")
          .WithColor(Color.Orange)
          .WithTimestamp(DateTimeOffset.UtcNow)
          .AddField("Attempts Made", skillCheck.Attempts.Count.ToString(), true)
          .AddField("Successful Attempts", skillCheck.Attempts.Count(a => a.Success).ToString(), true);

      await context.Interaction.FollowupAsync(embed: embed.Build());

      _logger.LogInformation($"Skill check '{skillName}' removed from guild {guildId}, channel {channelId}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing skill check");
      await context.Interaction.FollowupAsync("❌ An error occurred while removing the skill check.", ephemeral: true);
    }
  }

  /// <summary>
  /// Lists all active skill checks in the current channel
  /// </summary>
  public async Task ListSkillChecks(SocketInteractionContext context)
  {
    try
    {
      await context.Interaction.DeferAsync();

      var guildId = context.Guild?.Id ?? throw new InvalidOperationException("This command can only be used in a server.");
      var channelId = context.Channel.Id;

      var skillChecks = await _db.SkillChecks
          .Where(sc => sc.GuildId == guildId &&
                     sc.ChannelId == channelId &&
                     sc.IsActive &&
                     (sc.ExpiresAt == null || sc.ExpiresAt > DateTime.UtcNow))
          .Include(sc => sc.Attempts)
          .OrderBy(sc => sc.CreatedAt)
          .ToListAsync();

      if (!skillChecks.Any())
      {
        await context.Interaction.FollowupAsync("📝 No active skill checks in this channel.", ephemeral: true);
        return;
      }

      var embed = new EmbedBuilder()
          .WithTitle("📋 Active Skill Checks")
          .WithDescription($"Found {skillChecks.Count} active skill check(s) in this channel")
          .WithColor(Color.Green)
          .WithTimestamp(DateTimeOffset.UtcNow);

      foreach (var skillCheck in skillChecks)
      {
        var fieldValue = $"**Attempts:** {skillCheck.Attempts.Count}\n" +
                       $"**Created:** <t:{((DateTimeOffset)skillCheck.CreatedAt).ToUnixTimeSeconds()}:R>";

        if (skillCheck.IsPrivate)
          fieldValue += "\n🔒 **Private** (results via DM)";

        if (skillCheck.ExpiresAt.HasValue)
          fieldValue += $"\n**Expires:** <t:{((DateTimeOffset)skillCheck.ExpiresAt.Value).ToUnixTimeSeconds()}:R>";

        embed.AddField(skillCheck.SkillName, fieldValue, true);
      }

      await context.Interaction.FollowupAsync(embed: embed.Build());
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error listing skill checks");
      await context.Interaction.FollowupAsync("❌ An error occurred while listing skill checks.", ephemeral: true);
    }
  }

  /// <summary>
  /// Cleanup expired skill checks
  /// </summary>
  public async Task CleanupExpiredSkillChecks()
  {
    try
    {
      var expiredSkillChecks = await _db.SkillChecks
          .Where(sc => sc.IsActive &&
                     sc.ExpiresAt.HasValue &&
                     sc.ExpiresAt < DateTime.UtcNow)
          .ToListAsync();

      foreach (var skillCheck in expiredSkillChecks)
      {
        skillCheck.IsActive = false;
      }

      if (expiredSkillChecks.Any())
      {
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Cleaned up {expiredSkillChecks.Count} expired skill checks");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during skill check cleanup");
    }
  }
}
