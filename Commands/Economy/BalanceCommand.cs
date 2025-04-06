using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class BalanceCommand : BaseEconomyModule
  {
    public BalanceCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("balance", "Check your balance for a specific currency.")]
    public async Task CheckBalance(string currencyName)
    {
      var guildId = Context.Guild.Id;
      var userId = Context.User.Id;
      var username = Context.User.Username;

      var userRecord = await Db.Users.FirstOrDefaultAsync(u => u.DiscordId == userId);
      if (userRecord == null)
      {
        await RespondAsync("❌ You don't have a user record yet. Try using `/daily` first.");
        return;
      }

      var currency = await Db.CurrencyTypes
          .FirstOrDefaultAsync(c =>
              c.GuildId == guildId &&
              c.Name.ToLower() == currencyName.ToLower());

      if (currency == null)
      {
        await RespondAsync($"❌ Currency `{currencyName}` does not exist in this server.");
        return;
      }

      var balance = await Db.CurrencyBalances
          .FirstOrDefaultAsync(b =>
              b.UserId == userRecord.Id &&
              b.CurrencyTypeId == currency.Id &&
              b.GuildId == guildId);

      int amount = balance?.Amount ?? 0;

      var embed = new EmbedBuilder()
          .WithTitle($"💰 {currency.Name} Balance")
          .WithDescription($"{username}, you currently have **{amount} {currency.Name}**.")
          .WithColor(Color.Teal)
          .WithTimestamp(DateTime.UtcNow)
          .Build();

      await RespondAsync(embed: embed);
    }
  }
}
