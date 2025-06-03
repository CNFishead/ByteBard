using Discord.Interactions;

namespace FallVerseBotV2.Commands.Admin
{
  // Shared base for all admin-related commands
  public abstract class BaseGroupModule : InteractionModuleBase<SocketInteractionContext>
  {
    protected readonly ILogger Logger;
    protected readonly BotDbContext Db;

    public BaseGroupModule(ILogger<BaseGroupModule> logger, BotDbContext db)
    {
      Logger = logger;
      Db = db;
    }
  }

}