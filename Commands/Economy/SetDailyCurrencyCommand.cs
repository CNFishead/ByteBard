using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class SetDailyCurrencyCommand : BaseEconomyModule
  {
    public SetDailyCurrencyCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("setdailycurrency", "Set which currency users will receive from the /daily command.")]
    public async Task SetDailyCurrency(string currencyName)
    {
      try
      {

        await DeferAsync(true);
        var guildId = Context.Guild.Id;

        // Look for a currency that belongs to this guild
        var currency = await Db.CurrencyTypes
            .FirstOrDefaultAsync(c =>
                c.GuildId == guildId &&
                c.Name.ToLower() == currencyName.ToLower());

        if (currency == null)
        {
          await FollowupAsync($"❌ Currency `{currencyName}` does not exist in this server.");
          return;
        }

        // Find or create server settings
        var settings = await Db.ServerSettings.FindAsync(guildId);

        if (settings == null)
        {
          settings = new ServerSettings
          {
            GuildId = guildId,
            DailyCurrencyId = currency.Id
          };
          Db.ServerSettings.Add(settings);
        }
        else
        {
          settings.DailyCurrencyId = currency.Id;
        }

        await Db.SaveChangesAsync();
        await FollowupAsync($"✅ `{currencyName}` is now the daily currency for this server.");
      }
      catch (System.Exception ex)
      {
        {
          Logger.LogError(ex, "Error in SetDailyCommand: {Message}", ex.Message);
          await FollowupAsync("❌ Something went wrong while processing your request.");
          throw; // Rethrow the exception to be handled by the global exception handler
        }
      }
    }
  }
}
