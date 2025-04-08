using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy.Casino
{
  public enum CoinSide
  {
    Heads,
    Tails
  }

  public class CoinFlipCommand : BaseCasinoModule
  {
    public CoinFlipCommand(ILogger<BaseCasinoModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("flip", "Bet on heads or tails.")]
    public async Task CoinFlip(CoinSide choice, int amount)
    {
      try
      {
        await DeferAsync(true);
        var guildId = Context.Guild.Id;
        var discordUserId = Context.User.Id;

        // Step 1: Load server settings and casino currency
        var settings = await Db.ServerSettings
            .Include(s => s.CasinoCurrency)
            .FirstOrDefaultAsync(s => s.GuildId == guildId);

        if (settings?.CasinoCurrency == null)
        {
          await FollowupAsync("⚠️ This server has not set a casino currency yet. Use `/casino setcurrency`.");
          return;
        }

        var currency = settings.CasinoCurrency;

        // Step 2: Get or create the user record
        var userRecord = await Db.Users
            .FirstOrDefaultAsync(u => u.DiscordId == discordUserId);

        if (userRecord == null)
        {
          await FollowupAsync("⚠️ You don't have a profile yet. Try using `/daily` to initialize.");
          return;
        }

        // Step 3: Retrieve user's balance for this currency
        var userBalance = await Db.CurrencyBalances
            .FirstOrDefaultAsync(b => b.UserId == userRecord.Id &&
                                      b.CurrencyTypeId == currency.Id &&
                                      b.GuildId == guildId);

        if (userBalance == null || userBalance.Amount < amount)
        {
          await FollowupAsync($"❌ You don't have enough {currency.Name} to bet {amount}.");
          return;
        }

        // Step 4: Flip the coin 
        var result = SimulateCoinFlip(25); // Simulate 25 flips for better randomness

        string outcomeMessage;

        if (result == choice)
        {
          userBalance.Amount += amount; // Win: +amount
          outcomeMessage = $"🎉 You guessed **{choice}** and it landed on **{result}**!\nYou won `{amount}` {currency.Name}!";
        }
        else
        {
          userBalance.Amount -= amount; // Lose: -amount
          outcomeMessage = $"😢 You guessed **{choice}** but it landed on **{result}**.\nYou lost `{amount}` {currency.Name}.";
        }

        await Db.SaveChangesAsync();
        await FollowupAsync(outcomeMessage);
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Error in CoinFlipCommand: {Message}", ex.Message);
        await FollowupAsync("❌ An error occurred while processing your coin flip.");
        throw;
      }
    }
    private static CoinSide SimulateCoinFlip(int trials = 25)
    {
      var rng = new Random();
      int heads = 0;
      int tails = 0;

      for (int i = 0; i < trials; i++)
      {
        if (rng.Next(0, 2) == 0)
          tails++;
        else
          heads++;
      }

      // Tie-breaker: choose randomly
      if (heads == tails)
        return rng.Next(0, 2) == 0 ? CoinSide.Tails : CoinSide.Heads;

      return heads > tails ? CoinSide.Heads : CoinSide.Tails;
    }
  }
}
