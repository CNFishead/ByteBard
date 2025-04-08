using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class SetCasinoCurrencyCommand : BaseCasinoModule
  {
    public SetCasinoCurrencyCommand(ILogger<BaseCasinoModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("setcurrency", "Sets the currency to be used for casino games.")]
    public async Task SetCasinoCurrency(string currency)
    {
      try
      {
        await DeferAsync(true);
        var guildId = Context.Guild.Id;
        var currencyLower = currency.ToLower();

        var currencyEntity = await Db.CurrencyTypes
          .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name.ToLower() == currencyLower);

        if (currencyEntity == null)
        {
          await FollowupAsync($"❌ Currency `{currency}` does not exist in this server.");
          return;
        }

        var settings = await Db.ServerSettings
          .FirstOrDefaultAsync(s => s.GuildId == guildId);

        if (settings == null)
        {
          settings = new ServerSettings
          {
            GuildId = guildId,
            CasinoCurrency = currencyEntity
          };
          Db.ServerSettings.Add(settings);
        }
        else
        {
          settings.CasinoCurrency = currencyEntity;
        }

        await Db.SaveChangesAsync();

        await FollowupAsync($"🎰 Casino currency has been set to `{currency}`.");
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Error in SetCasinoCurrencyCommand: {Message}", ex.Message);
        await FollowupAsync("❌ Failed to set casino currency due to an error.");
        throw;
      }
    }
  }
}
