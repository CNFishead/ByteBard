using Discord.Interactions;

namespace FallVerseBotV2.Commands.Help
{
  /// <summary>
  /// Base class for all help-related command modules
  /// </summary>
  public abstract class BaseHelpModule : InteractionModuleBase<SocketInteractionContext>
  {
    protected readonly ILogger Logger;
    protected readonly BotDbContext Db;

    public BaseHelpModule(ILogger<BaseHelpModule> logger, BotDbContext db)
    {
      Logger = logger;
      Db = db;
    }
  }
}
