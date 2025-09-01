
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class SkillCheckHandler
{
  private readonly ILogger<SkillCheckHandler> _logger;
  private readonly BotDbContext _db;

  public SkillCheckHandler(ILogger<SkillCheckHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  /// <summary>
  /// Handles a player's skill check roll attempt
  /// </summary>
  public async Task HandleSkillRoll(SocketInteractionContext context, string skillName, int rollResult)
  {
    try
    {
      await context.Interaction.DeferAsync();

      var guildId = context.Guild?.Id ?? throw new InvalidOperationException("This command can only be used in a server.");
      var channelId = context.Channel.Id;
      var userId = context.User.Id;

      // Find the active skill check
      var skillCheck = await _db.SkillChecks
          .Include(sc => sc.Attempts)
          .Where(sc => sc.GuildId == guildId &&
                     sc.ChannelId == channelId &&
                     sc.SkillName.ToLower() == skillName.ToLower() &&
                     sc.IsActive &&
                     (sc.ExpiresAt == null || sc.ExpiresAt > DateTime.UtcNow))
          .FirstOrDefaultAsync();

      if (skillCheck == null)
      {
        await context.Interaction.FollowupAsync(
            $"❌ No active **{skillName}** skill check found in this channel.",
            ephemeral: true);
        return;
      }

      // Check if user has already attempted this skill check
      var existingAttempt = skillCheck.Attempts.FirstOrDefault(a => a.UserId == userId);
      if (existingAttempt != null)
      {
        await context.Interaction.FollowupAsync(
            $"⚠️ You have already attempted the **{skillName}** skill check (rolled {existingAttempt.RollResult}).",
            ephemeral: true);
        return;
      }

      // Process the roll
      bool success = rollResult >= skillCheck.DC;

      // Create the attempt record
      var attempt = new SkillCheckAttempt
      {
        SkillCheckId = skillCheck.Id,
        UserId = userId,
        RollResult = rollResult,
        Success = success
      };

      _db.SkillCheckAttempts.Add(attempt);
      await _db.SaveChangesAsync();

      // Handle private vs public results
      if (skillCheck.IsPrivate)
      {
        // For private skill checks, only send result via DM
        try
        {
          var dmEmbed = new EmbedBuilder()
              .WithTitle($"🎲 {skillName} Skill Check Result")
              .WithDescription($"**Roll:** {rollResult} vs **DC:** {skillCheck.DC}")
              .WithTimestamp(DateTimeOffset.UtcNow);

          if (success)
          {
            dmEmbed.WithColor(Color.Green)
                   .AddField("Result", "✅ **SUCCESS!**", false);

            if (!string.IsNullOrWhiteSpace(skillCheck.SuccessMessage))
            {
              dmEmbed.AddField("Success", skillCheck.SuccessMessage, false);
            }
          }
          else
          {
            dmEmbed.WithColor(Color.Red)
                   .AddField("Result", "❌ **FAILURE**", false);

            if (!string.IsNullOrWhiteSpace(skillCheck.FailureMessage))
            {
              dmEmbed.AddField("Failure", skillCheck.FailureMessage, false);
            }
          }

          await context.User.SendMessageAsync(embed: dmEmbed.Build());

          // Send a simple confirmation to the channel without revealing results
          await context.Interaction.FollowupAsync(
              $"🎲 {context.User.Mention} attempted the **{skillName}** skill check. Result sent privately.",
              ephemeral: false);
        }
        catch (Exception dmEx)
        {
          _logger.LogWarning(dmEx, $"Could not send DM to user {userId} for private skill check");
          await context.Interaction.FollowupAsync(
              "❌ Unable to send private result. Please enable DMs from server members.",
              ephemeral: true);
        }
      }
      else
      {
        // For public skill checks, send full result to channel including success/failure messages
        var embed = new EmbedBuilder()
            .WithTitle($"🎲 {skillName} Skill Check Result")
            .WithDescription($"**Roll:** {rollResult}")
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter($"Attempt by {context.User.Username}");

        if (success)
        {
          embed.WithColor(Color.Green)
               .AddField("Result", "✅ **SUCCESS!**", false);

          // Add success message to public embed if available
          if (!string.IsNullOrWhiteSpace(skillCheck.SuccessMessage))
          {
            embed.AddField("Success", skillCheck.SuccessMessage, false);
          }
        }
        else
        {
          embed.WithColor(Color.Red)
               .AddField("Result", "❌ **FAILURE**", false);

          // Add failure message to public embed if available
          if (!string.IsNullOrWhiteSpace(skillCheck.FailureMessage))
          {
            embed.AddField("Failure", skillCheck.FailureMessage, false);
          }
        }

        // Send the result publicly (no DM needed for public skill checks)
        await context.Interaction.FollowupAsync(embed: embed.Build());
      }

      _logger.LogInformation($"User {userId} rolled {rollResult} for {skillName} skill check (DC {skillCheck.DC}) - {(success ? "SUCCESS" : "FAILURE")}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling skill check roll");
      await context.Interaction.FollowupAsync("❌ An error occurred while processing your skill check.", ephemeral: true);
    }
  }

  /// <summary>
  /// Shows the status of a specific skill check
  /// </summary>
  public async Task ShowSkillCheckStatus(SocketInteractionContext context, string skillName)
  {
    try
    {
      await context.Interaction.DeferAsync();

      var guildId = context.Guild?.Id ?? throw new InvalidOperationException("This command can only be used in a server.");
      var channelId = context.Channel.Id;
      var userId = context.User.Id;

      var skillCheck = await _db.SkillChecks
          .Include(sc => sc.Attempts)
          .Where(sc => sc.GuildId == guildId &&
                     sc.ChannelId == channelId &&
                     sc.SkillName.ToLower() == skillName.ToLower() &&
                     sc.IsActive &&
                     (sc.ExpiresAt == null || sc.ExpiresAt > DateTime.UtcNow))
          .FirstOrDefaultAsync();

      if (skillCheck == null)
      {
        await context.Interaction.FollowupAsync(
            $"❌ No active **{skillName}** skill check found in this channel.",
            ephemeral: true);
        return;
      }

      // Check if user is the creator (can see all details) or a regular player (limited info)
      bool isCreator = skillCheck.CreatedByUserId == userId;

      var embed = new EmbedBuilder()
          .WithTitle($"📊 {skillName} Skill Check Status")
          .WithColor(Color.Blue)
          .WithTimestamp(DateTimeOffset.UtcNow)
          .AddField("Total Attempts", skillCheck.Attempts.Count.ToString(), true)
          .AddField("Successful Attempts", skillCheck.Attempts.Count(a => a.Success).ToString(), true)
          .AddField("Created", $"<t:{((DateTimeOffset)skillCheck.CreatedAt).ToUnixTimeSeconds()}:R>", true);

      // Only show DC to the creator
      if (isCreator)
      {
        embed.WithDescription($"**DC:** {skillCheck.DC}");

        if (skillCheck.IsPrivate)
          embed.AddField("Privacy", "🔒 Private (results via DM)", true);
      }
      else
      {
        if (skillCheck.IsPrivate)
          embed.WithDescription("🔒 Private skill check (results via DM)");
      }

      if (skillCheck.ExpiresAt.HasValue)
        embed.AddField("Expires", $"<t:{((DateTimeOffset)skillCheck.ExpiresAt.Value).ToUnixTimeSeconds()}:R>", true);

      // Only show messages to creator
      if (isCreator)
      {
        if (!string.IsNullOrWhiteSpace(skillCheck.SuccessMessage))
          embed.AddField("Success Message", skillCheck.SuccessMessage, false);

        if (!string.IsNullOrWhiteSpace(skillCheck.FailureMessage))
          embed.AddField("Failure Message", skillCheck.FailureMessage, false);
      }

      // Show recent attempts (last 5) - but hide roll results for private skill checks unless you're the creator
      var recentAttempts = skillCheck.Attempts
          .OrderByDescending(a => a.AttemptedAt)
          .Take(5)
          .ToList();

      if (recentAttempts.Any())
      {
        string attemptsText;
        if (skillCheck.IsPrivate && !isCreator)
        {
          // For private skill checks, only show who attempted, not results
          attemptsText = string.Join("\n", recentAttempts.Select(a =>
              $"<@{a.UserId}>: Attempted"));
        }
        else
        {
          // For public skill checks or if you're the creator, show full results
          attemptsText = string.Join("\n", recentAttempts.Select(a =>
              $"<@{a.UserId}>: {a.RollResult} {(a.Success ? "✅" : "❌")}"));
        }
        embed.AddField("Recent Attempts", attemptsText, false);
      }

      await context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: !isCreator);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error showing skill check status");
      await context.Interaction.FollowupAsync("❌ An error occurred while retrieving skill check status.", ephemeral: true);
    }
  }
}