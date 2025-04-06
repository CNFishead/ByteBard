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
      var guildId = Context.Guild.Id;
      var lowerName = name.ToLower();

      // Ensure the currency does not already exist in this guild
      var exists = await Db.CurrencyTypes
          .AnyAsync(c => c.GuildId == guildId && c.Name.ToLower() == lowerName);

      if (exists)
      {
        await RespondAsync($"❌ Currency `{name}` already exists in this server.");
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

      await RespondAsync($"✅ Currency `{name}` has been added to this server.");
    }
  }
}
