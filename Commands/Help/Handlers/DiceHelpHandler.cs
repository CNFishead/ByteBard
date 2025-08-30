using Discord;
using Discord.Interactions;

namespace FallVerseBotV2.Commands.Help.Handlers
{
  /// <summary>
  /// Handler for dice rolling help functionality
  /// </summary>
  public class DiceHelpHandler
  {
    private readonly ILogger<DiceHelpHandler> _logger;
    private readonly BotDbContext _db;

    public DiceHelpHandler(ILogger<DiceHelpHandler> logger, BotDbContext db)
    {
      _logger = logger;
      _db = db;
    }

    /// <summary>
    /// Displays comprehensive help for the dice rolling system
    /// </summary>
    public async Task Run(SocketInteractionContext context)
    {
      try
      {
        _logger.LogInformation($"DiceHelp requested by user {context.User.Id} in guild {context.Guild?.Id}");

        var embed = new EmbedBuilder()
            .WithTitle("🎲 Advanced Dice Rolling System")
            .WithDescription("Learn how to use the powerful dice rolling features!")
            .WithColor(Color.Blue)
            .WithThumbnailUrl("https://cdn.discordapp.com/emojis/🎲.png")
            .WithTimestamp(DateTimeOffset.Now);

        // Basic Usage
        embed.AddField("📋 Basic Usage",
            "```/roll 1d20+3```\n" +
            "```/roll 3d6 \"damage roll\"```\n" +
            "```/roll 2d8+2; 1d4+1 \"multiple rolls\"```",
            false);

        // Advantage/Disadvantage
        embed.AddField("⚖️ Advantage & Disadvantage",
            "```/roll 1d20+5 adv``` - Roll with advantage\n" +
            "```/roll 1d20+5 disadv``` - Roll with disadvantage\n" +
            "```/roll 1d20+3 \"attack\" adv``` - With label",
            false);

        // PEMDAS Operations
        embed.AddField("🧮 Mathematical Operations (PEMDAS)",
            "```/roll 3d6x2+1``` - Roll 3d6, multiply by 2, add 1\n" +
            "```/roll 2d8+5*3-2``` - Complex math expressions\n" +
            "```/roll 1d10*2/4+3``` - Division and multiplication\n" +
            "**Operators:** `+` `-` `*` `x` `/`",
            false);

        // Keep Best/Worst
        embed.AddField("🏆 Keep Best/Worst Rolls",
            "```/roll 4d6b3``` - Roll 4d6, keep best 3\n" +
            "```/roll 4d6w3``` - Roll 4d6, keep worst 3\n" +
            "```/roll 6d8b4 \"character stats\"``` - With label\n" +
            "**Example:** `[4,6,3,1]` → best 3 → `[6,4,3]` = 13",
            false);

        // Combined Features
        embed.AddField("🔧 Combined Features",
            "```/roll 4d6b3x2+5 \"enhanced stats\"```\n" +
            "Roll 4d6, keep best 3, multiply by 2, add 5\n" +
            "```/roll 3d8w2+10 \"penalty roll\"```\n" +
            "All features work together seamlessly!",
            false);

        // Multiple Expressions
        embed.AddField("📝 Multiple Expressions & Labels",
            "```/roll 1d20+5 \"attack\"; 2d6+3 \"damage\"```\n" +
            "```/roll 3d6b3 \"str\"; 3d6b3 \"dex\"; 3d6b3 \"con\"```\n" +
            "Separate with `;` | Add labels with `\"quotes\"`",
            false);

        // Examples Section
        embed.AddField("💡 Real-World Examples",
            "**Character Creation:**\n" +
            "```/roll 4d6b3; 4d6b3; 4d6b3; 4d6b3; 4d6b3; 4d6b3```\n" +
            "**Critical Hit:**\n" +
            "```/roll 1d20+8 \"attack\" adv; 4d6+4x2 \"crit damage\"```\n" +
            "**Fireball Damage:**\n" +
            "```/roll 8d6 \"fireball damage\"```",
            false);

        embed.WithFooter("💡 Tip: The bot automatically detects when to use advanced features based on your expression!");

        await context.Interaction.RespondAsync(embed: embed.Build());

        _logger.LogInformation($"DiceHelp successfully sent to user {context.User.Id}");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error in DiceHelp for user {context.User.Id}");
        await context.Interaction.RespondAsync("❌ Something went wrong while displaying the help.", ephemeral: true);
        throw;
      }
    }
  }
}
