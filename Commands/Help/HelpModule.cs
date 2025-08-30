using Discord.Interactions;
using FallVerseBotV2.Commands.Help.Handlers;
using Microsoft.Extensions.Logging;

namespace FallVerseBotV2.Commands.Help
{
  [Group("help", "Help-related commands for bot features and functionality.")]
  public class HelpModule : BaseHelpModule
  {
    private readonly ILoggerFactory _loggerFactory;
    private readonly DiceHelpHandler _diceHelpHandler;

    public HelpModule(ILogger<BaseHelpModule> logger, ILoggerFactory loggerFactory, BotDbContext db)
        : base(logger, db)
    {
      _loggerFactory = loggerFactory;
      _diceHelpHandler = new DiceHelpHandler(_loggerFactory.CreateLogger<DiceHelpHandler>(), db);

      // Log module construction for debugging
      logger.LogInformation("HelpModule constructed successfully");
    }

    [SlashCommand("dice", "Display comprehensive help for the dice rolling system")]
    public async Task DiceHelp() => await _diceHelpHandler.Run(Context);

    [SlashCommand("commands", "Display a list of all available bot commands")]
    public async Task CommandsHelp()
    {
      // TODO: Implement general commands help
      await RespondAsync("🚧 General commands help coming soon! For now, try `/help dice` for dice rolling help.", ephemeral: true);
    }

    [SlashCommand("economy", "Display help for economy and currency commands")]
    public async Task EconomyHelp()
    {
      // TODO: Implement economy help
      await RespondAsync("🚧 Economy help coming soon! For now, explore `/casino` and economy commands.", ephemeral: true);
    }

    [SlashCommand("admin", "Display help for administrative commands")]
    public async Task AdminHelp()
    {
      // TODO: Implement admin help
      await RespondAsync("🚧 Admin help coming soon! For now, explore `/admin` commands.", ephemeral: true);
    }
  }
}
