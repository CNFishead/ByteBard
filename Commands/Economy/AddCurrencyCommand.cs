using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class AddCurrencyTypeCommand : BaseEconomyModule
  {
    public AddCurrencyTypeCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("addcurrencytype", "Adds a new currency type to this server.")]
    public async Task AddCurrencyType(string name)
    {
      try
      {

        await DeferAsync(true);
        var guildId = Context.Guild.Id;
        var lowerName = name.ToLower();

        // Ensure the currency does not already exist in this guild
        var exists = await Db.CurrencyTypes
            .AnyAsync(c => c.GuildId == guildId && c.Name.ToLower() == lowerName);

        if (exists)
        {
          await FollowupAsync($"❌ Currency `{name}` already exists in this server.");
          return;
        }

        // Create new currency
        var currency = new CurrencyType
        {
          Name = name,
          GuildId = guildId
        };

        Db.CurrencyTypes.Add(currency);
        await Db.SaveChangesAsync();

        await FollowupAsync($"✅ Currency `{name}` has been added to this server.");

      }
      catch (System.Exception ex)
      {
        {
          Logger.LogError(ex, "Error in AddCurrencyCommand: {Message}", ex.Message);
          await FollowupAsync("❌ Something went wrong while processing your request.");
          throw; // Rethrow the exception to be handled by the global exception handler
        }
      }
    }
  }
}
