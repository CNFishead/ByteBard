using Discord.Interactions;

namespace FallVerseBotV2.Commands.Economy
{ 
  // group decorator for all casino-related commands
  [Group("casino", "Casino-related commands.")]
  [EnabledInDm(false)]
  public abstract class BaseCasinoModule : InteractionModuleBase<SocketInteractionContext>
  {
    protected readonly ILogger Logger;
    protected readonly BotDbContext Db;

    public BaseCasinoModule(ILogger<BaseCasinoModule> logger, BotDbContext db)
    {
      Logger = logger;
      Db = db;
    }
  }
}
