using Discord.Interactions;

namespace FallVerseBotV2.Commands.Economy
{
    // Shared base for all economy-related commands
    public abstract class BaseEconomyModule : InteractionModuleBase<SocketInteractionContext>
    {
        protected readonly ILogger Logger;
        protected readonly BotDbContext Db;

        public BaseEconomyModule(ILogger<BaseEconomyModule> logger, BotDbContext db)
        {
            Logger = logger;
            Db = db;
        }
    }
}
