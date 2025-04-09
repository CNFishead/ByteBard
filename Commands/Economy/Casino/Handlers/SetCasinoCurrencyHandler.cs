using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy.Casino.Handlers
{
    public class SetCasinoCurrencyHandler
    {
        private readonly ILogger _logger;
        private readonly BotDbContext _db;

        public SetCasinoCurrencyHandler(ILogger logger, BotDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task Run(SocketInteractionContext context, string currencyName)
        {
            try
            {
                await context.Interaction.DeferAsync(true);

                var guildId = context.Guild.Id;
                var currencyLower = currencyName.ToLower();

                var currencyEntity = await _db.CurrencyTypes
                    .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name.ToLower() == currencyLower);

                if (currencyEntity == null)
                {
                    await context.Interaction.FollowupAsync($"❌ Currency `{currencyName}` does not exist in this server.");
                    return;
                }

                var settings = await _db.ServerSettings
                    .FirstOrDefaultAsync(s => s.GuildId == guildId);

                if (settings == null)
                {
                    settings = new ServerSettings
                    {
                        GuildId = guildId,
                        CasinoCurrency = currencyEntity
                    };
                    _db.ServerSettings.Add(settings);
                }
                else
                {
                    settings.CasinoCurrency = currencyEntity;
                }

                await _db.SaveChangesAsync();
                await context.Interaction.FollowupAsync($"🎰 Casino currency has been set to `{currencyName}`.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetCasinoCurrencyHandler: {Message}", ex.Message);
                await context.Interaction.FollowupAsync("❌ Failed to set casino currency due to an error.");
                throw;
            }
        }
    }
}
