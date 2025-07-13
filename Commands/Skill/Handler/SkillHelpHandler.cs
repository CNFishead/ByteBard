using Discord;
using Discord.Interactions;

public class SkillHelpHandler
{
  private readonly ILogger<SkillHelpHandler> _logger;

  public SkillHelpHandler(ILogger<SkillHelpHandler> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Shows comprehensive help for skill check commands
  /// </summary>
  public async Task ShowHelp(SocketInteractionContext context)
  {
    try
    {
      var embed = new EmbedBuilder()
          .WithTitle("🎲 Skill Check System - Help")
          .WithDescription("A comprehensive system for managing skill checks in your Discord server.")
          .WithColor(Color.Blue)
          .WithTimestamp(DateTimeOffset.UtcNow)
          .WithFooter("Use these commands to create engaging skill-based gameplay!");

      // Init command
      embed.AddField("📝 `/skill init-skillcheck`",
          "**Create a new skill check**\n" +
          "• `skill` - Name of the skill (e.g., Perception)\n" +
          "• `dc` - Difficulty Class (number to beat)\n" +
          "• `successMessage` - Message for success (optional)\n" +
          "• `failureMessage` - Message for failure (optional)\n" +
          "• `durationMinutes` - Auto-expire time (optional)\n" +
          "• `isPrivate` - Send results only via DM (optional)\n" +
          "Example: `/skill init-skillcheck skill:Stealth dc:15 isPrivate:true`", false);

      // Roll command
      embed.AddField("🎲 `/skill roll`",
          "**Attempt a skill check**\n" +
          "• `skill` - Name of the skill check to attempt\n" +
          "• `rollResult` - Your dice roll result\n" +
          "Example: `/skill roll skill:Stealth rollResult:18`\n" +
          "Note: Each player can only attempt once per skill check", false);

      // List command
      embed.AddField("📋 `/skill list-skillchecks`",
          "**View active skill checks**\n" +
          "Shows all active skill checks in this channel with basic information.\n" +
          "DCs and messages are hidden for security.", false);

      // Status command
      embed.AddField("📊 `/skill status`",
          "**Check skill check details**\n" +
          "• `skill` - Name of the skill check to view\n" +
          "Shows attempt statistics and recent rolls.\n" +
          "Creators see full details, players see limited info.", false);

      // Remove command
      embed.AddField("🗑️ `/skill remove-skillcheck`",
          "**Remove a skill check**\n" +
          "• `skill` - Name of the skill check to remove\n" +
          "Deactivates the skill check and shows final statistics.", false);

      await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);

      _logger.LogInformation($"Help command executed by user {context.User.Id} in guild {context.Guild?.Id}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error showing skill check help");
      await context.Interaction.RespondAsync("❌ An error occurred while displaying help.", ephemeral: true);
    }
  }

  /// <summary>
  /// Shows privacy and security information
  /// </summary>
  public async Task ShowPrivacyHelp(SocketInteractionContext context)
  {
    try
    {
      var embed = new EmbedBuilder()
          .WithTitle("🔒 Skill Check Privacy & Security")
          .WithDescription("Understanding how information is protected in skill checks.")
          .WithColor(Color.Gold)
          .WithTimestamp(DateTimeOffset.UtcNow);

      embed.AddField("🌐 Public Mode (default)",
          "• Channel shows: Roll result + Success/Failure ✅❌\n" +
          "• DM contains: Detailed success/failure messages\n" +
          "• Good for: General skill checks where outcome can be public", false);

      embed.AddField("🔒 Private Mode (isPrivate:true)",
          "• Channel shows: \"Player attempted. Result sent privately.\"\n" +
          "• DM contains: Roll, DC, result, and detailed messages\n" +
          "• Good for: Secret information, stealth, investigation", false);

      embed.AddField("🛡️ Information Security",
          "• **DCs are NEVER shown publicly** - Only to creators\n" +
          "• **Success/failure messages sent via DM** - Keeps secrets safe\n" +
          "• **Status command** shows different info based on permissions\n" +
          "• **List command** hides sensitive information from players", false);

      embed.AddField("👑 Creator vs Player Views",
          "**Creators can see:**\n" +
          "• Full skill check details including DC\n" +
          "• All success/failure messages\n" +
          "• Complete attempt history with roll results\n\n" +
          "**Players can see:**\n" +
          "• Basic statistics (attempt counts)\n" +
          "• Limited recent attempts\n" +
          "• No DCs or messages", false);

      await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);

      _logger.LogInformation($"Privacy help command executed by user {context.User.Id} in guild {context.Guild?.Id}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error showing privacy help");
      await context.Interaction.RespondAsync("❌ An error occurred while displaying privacy help.", ephemeral: true);
    }
  }

  /// <summary>
  /// Shows practical examples of skill check usage
  /// </summary>
  public async Task ShowExamples(SocketInteractionContext context)
  {
    try
    {
      var embed = new EmbedBuilder()
          .WithTitle("📚 Skill Check Examples")
          .WithDescription("Common scenarios and how to set them up.")
          .WithColor(Color.Green)
          .WithTimestamp(DateTimeOffset.UtcNow);

      embed.AddField("🔍 Basic Perception Check",
          "```/skill init-skillcheck skill:Perception dc:15```\n" +
          "Simple skill check with no custom messages.", false);

      embed.AddField("🗝️ Investigation with Messages",
          "```/skill init-skillcheck skill:Investigation dc:20 successMessage:\"You find a hidden key!\" failureMessage:\"Nothing unusual here.\"```\n" +
          "Provides specific feedback for success and failure.", false);

      embed.AddField("🥷 Secret Stealth Check",
          "```/skill init-skillcheck skill:Stealth dc:18 successMessage:\"You overhear their plans.\" isPrivate:true```\n" +
          "Perfect for gathering secret information without revealing to others.", false);

      embed.AddField("⏰ Temporary Athletics Check",
          "```/skill init-skillcheck skill:Athletics dc:16 durationMinutes:30```\n" +
          "Auto-expires after 30 minutes to keep things moving.", false);

      embed.AddField("🎭 Complex Social Check",
          "```/skill init-skillcheck skill:Deception dc:22 successMessage:\"The guard believes your story completely.\" failureMessage:\"The guard looks suspicious.\" durationMinutes:60 isPrivate:true```\n" +
          "Full-featured private skill check with expiration.", false);

      embed.AddField("🎲 How Players Roll",
          "After a skill check is created, players attempt it:\n" +
          "```/skill roll skill:Perception rollResult:18```\n" +
          "```/skill roll skill:Stealth rollResult:12```", false);

      await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);

      _logger.LogInformation($"Examples help command executed by user {context.User.Id} in guild {context.Guild?.Id}");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error showing examples help");
      await context.Interaction.RespondAsync("❌ An error occurred while displaying examples.", ephemeral: true);
    }
  }
}
