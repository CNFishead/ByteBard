using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class ModifyCurrencyCommand : BaseEconomyModule
  {
    public ModifyCurrencyCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("modifycurrency", "Modify a user's currency of a specific type.")]
    public async Task ModifyCurrency(SocketUser user, string currencyName, int amount)
    {
      try
      {

        await DeferAsync();
        var guildId = Context.Guild.Id;
        var discordId = user.Id;
        var username = user.Username;

        // Fetch user record
        var userRecord = await Db.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
        if (userRecord == null)
        {
          await FollowupAsync("❌ That user has no record in the database.", ephemeral: true);
          return;
        }

        // Look up currency type scoped to this guild
        var currencyType = await Db.CurrencyTypes
            .FirstOrDefaultAsync(c =>
                c.GuildId == guildId &&
                c.Name.ToLower() == currencyName.ToLower());

        if (currencyType == null)
        {
          await FollowupAsync($"❌ Currency type `{currencyName}` does not exist in this server.", ephemeral: true);
          return;
        }

        // Look up or create user balance
        var balance = await Db.CurrencyBalances
            .FirstOrDefaultAsync(b =>
                b.UserId == userRecord.Id &&
                b.CurrencyTypeId == currencyType.Id &&
                b.GuildId == guildId);

        if (balance == null)
        {
          balance = new UserCurrencyBalance
          {
            UserId = userRecord.Id,
            CurrencyTypeId = currencyType.Id,
            GuildId = guildId,
            Amount = 0
          };
          Db.CurrencyBalances.Add(balance);
        }

        // Prevent negative balances
        if (balance.Amount + amount < 0)
        {
          await FollowupAsync("❌ This operation would result in a negative balance.", ephemeral: true);
          return;
        }

        balance.Amount += amount;
        await Db.SaveChangesAsync();
        await FollowupAsync($"✅ {user.Mention} now has {balance.Amount:N0} {currencyType.Name}.");
      }
      catch (System.Exception ex)
      {
        {
          Logger.LogError(ex, "Error in ModifyCurrencyCommand: {Message}", ex.Message);
          await FollowupAsync("❌ Something went wrong while processing your request.");
          throw; // Rethrow the exception to be handled by the global exception handler
        }
      }
    }
  }
}
