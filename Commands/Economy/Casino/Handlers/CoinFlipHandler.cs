using Discord.Interactions;
using FallVerseBotV2.Enums;
using Microsoft.EntityFrameworkCore;

public class CoinFlipHandler
{
  private readonly ILogger<CoinFlipHandler> _logger;
  private readonly BotDbContext _db;

  public CoinFlipHandler(ILogger<CoinFlipHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }
  public async Task Run(SocketInteractionContext context, CoinSide choice, int amount)
  {
    try
    {
      await context.Interaction.DeferAsync();
      var guildId = context.Guild.Id;
      var discordUserId = context.User.Id;

      // Step 1: Load server settings and casino currency
      var settings = await _db.ServerSettings
          .Include(s => s.CasinoCurrency)
          .FirstOrDefaultAsync(s => s.GuildId == guildId);

      if (settings?.CasinoCurrency == null)
      {
        await context.Interaction.FollowupAsync("⚠️ This server has not set a casino currency yet. Use `/casino setcurrency`.");
        return;
      }

      var currency = settings.CasinoCurrency;

      // Step 2: Get or create the user record
      var userRecord = await _db.Users
          .FirstOrDefaultAsync(u => u.DiscordId == discordUserId);

      if (userRecord == null)
      {
        await context.Interaction.FollowupAsync("⚠️ You don't have a profile yet. Try using `/daily` to initialize.");
        return;
      }

      // Step 3: Retrieve user's balance for this currency
      var userBalance = await _db.CurrencyBalances
          .FirstOrDefaultAsync(b => b.UserId == userRecord.Id &&
                                    b.CurrencyTypeId == currency.Id &&
                                    b.GuildId == guildId);

      if (userBalance == null || userBalance.Amount < amount)
      {
        await context.Interaction.FollowupAsync($"❌ You don't have enough {currency.Name} to bet {amount}.");
        return;
      }

      // Step 4: Flip the coin 
      int botBias = CalculateBotBias(amount, userBalance.Amount);
      var result = SimulateCoinFlip(choice, 25, botBias);


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

      await _db.SaveChangesAsync();
      await context.Interaction.FollowupAsync(outcomeMessage);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in CoinFlipCommand: {Message}", ex.Message);
      await context.Interaction.FollowupAsync("❌ An error occurred while processing your coin flip.");
      throw;
    }
  }
  private static CoinSide SimulateCoinFlip(CoinSide userChoice, int trials = 25, int botBias = 5)
  {
    var rng = new Random();
    int heads = 0;
    int tails = 0;

    // Add bot bias: pre-load wins for the *opposite* of user's choice
    if (userChoice == CoinSide.Heads)
      tails += botBias;
    else
      heads += botBias;

    for (int i = 0; i < trials - botBias; i++)
    {
      if (rng.Next(0, 2) == 0)
        tails++;
      else
        heads++;
    }

    if (heads == tails)
      return rng.Next(0, 2) == 0 ? CoinSide.Tails : CoinSide.Heads;

    return heads > tails ? CoinSide.Heads : CoinSide.Tails;
  }

  private static int CalculateBotBias(int betAmount, int userBalance)
  {
    if (userBalance == 0)
      return 1;

    double percent = (double)betAmount / userBalance;

    if (percent < 0.05) return 1;
    if (percent < 0.10) return 2;
    if (percent < 0.25) return 3;
    if (percent < 0.50) return 4;
    return 5; // Cap bias at 5
  }

}
