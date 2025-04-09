using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class DiceDuelHandler : IGameHandler
{
  private readonly ILogger<DiceDuelHandler> _logger;
  private readonly BotDbContext _db;
  public string GameKey => "diceduel";
  public DiceDuelHandler(ILogger<DiceDuelHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  public async Task Run(SocketInteractionContext context, int amount)
  {

    var userId = context.User.Id;
    var guildId = context.Guild.Id;
    const int dieFaces = 20; // Number of faces on the die

    await context.Interaction.DeferAsync();

    var settings = await _db.ServerSettings
        .Include(s => s.CasinoCurrency)
        .FirstOrDefaultAsync(s => s.GuildId == guildId);

    if (settings?.CasinoCurrency == null)
    {
      await context.Interaction.FollowupAsync("⚠️ This server has not set a casino currency yet.");
      return;
    }

    var currency = settings.CasinoCurrency;

    var userRecord = await _db.Users
        .FirstOrDefaultAsync(u => u.DiscordId == userId);

    if (userRecord == null)
    {
      await context.Interaction.FollowupAsync("⚠️ You don't have a profile yet. Use `/daily` to initialize.");
      return;
    }

    var balance = await _db.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == userRecord.Id &&
                                  b.CurrencyTypeId == currency.Id &&
                                  b.GuildId == guildId);
    if (balance == null || balance.Amount < amount)
    {
      await context.Interaction.FollowupAsync($"❌ You don't have enough {currency.Name} to bet {amount}.");
      return;
    }

    var userStartBalance = balance.Amount;
    int playerRoll = SimulatedDiceRoll(1, dieFaces);
    int botMinRoll = CalculateBotMinRoll(amount, balance.Amount, dieFaces);
    int botRoll = SimulatedDiceRoll(botMinRoll, dieFaces);

    // Handle 30% reroll chance on tie
    if (playerRoll == botRoll)
    {
      var rng = new Random();
      if (rng.NextDouble() < 0.30)
      {
        botRoll = SimulatedDiceRoll(botMinRoll, dieFaces); // reroll once
      }
    }


    string resultMessage;
    if (playerRoll > botRoll)
    {
      balance.Amount += amount;
      resultMessage = $"🎉 You rolled a **{playerRoll}**, bot rolled **{botRoll}**. You win `{amount}` {currency.Name}!";
    }
    else if (playerRoll < botRoll)
    {
      balance.Amount -= amount;
      resultMessage = $"😢 You rolled a **{playerRoll}**, bot rolled **{botRoll}**. You lose `{amount}` {currency.Name}.";
    }
    else
    {
      resultMessage = $"🤝 Tie! You both rolled **{playerRoll}**. No change.";
    }

    // fetch the users game stats
    var userGameStats = await _db.UserGameStats
        .FirstOrDefaultAsync(s => s.UserId == userRecord.Id && s.GuildId == guildId && s.GameKey == "diceduel");

    if (userGameStats == null)
    {
      // create a new game stats record
      userGameStats = new UserGameStats
      {
        UserId = userRecord.Id,
        GuildId = guildId,
        GameKey = GameKey,
        Wins = playerRoll > botRoll ? 1 : 0,
        Losses = playerRoll < botRoll ? 1 : 0,
        Ties = playerRoll == botRoll ? 1 : 0,
        TotalWagered = amount,
        NetGain = playerRoll > botRoll ? amount : -amount,
        LastPlayed = DateTime.UtcNow,
        LastGameData = new Dictionary<string, JsonElement>
        {
          ["amount"] = JsonSerializer.SerializeToElement(amount),
          ["dieFaces"] = JsonSerializer.SerializeToElement(dieFaces),
        }
      };
      _db.UserGameStats.Add(userGameStats);
    }

    // otherwise we update the userGameRecord
    else
    {
      userGameStats.Wins += playerRoll > botRoll ? 1 : 0;
      userGameStats.Losses += playerRoll < botRoll ? 1 : 0;
      userGameStats.Ties += playerRoll == botRoll ? 1 : 0;
      userGameStats.TotalWagered += amount;
      userGameStats.NetGain += playerRoll > botRoll ? amount : -amount;
      userGameStats.LastPlayed = DateTime.UtcNow;
      if (userGameStats.LastGameData == null)
        userGameStats.LastGameData = new Dictionary<string, JsonElement>();

      userGameStats.LastGameData["amount"] = JsonSerializer.SerializeToElement(amount);
      userGameStats.LastGameData["dieFaces"] = JsonSerializer.SerializeToElement(dieFaces);
    }

    // log the game data for debugging
    _logger.LogInformation("Saving UserGameStats: {@Stats}", new
    {
      userGameStats.UserId,
      userGameStats.GuildId,
      userGameStats.GameKey,
      userGameStats.Wins,
      userGameStats.Losses,
      userGameStats.Ties,
      userGameStats.NetGain,
      userGameStats.TotalWagered,
      userGameStats.LastPlayed,
      LastGameData = JsonSerializer.Serialize(userGameStats.LastGameData)
    });



    await _db.SaveChangesAsync();

    // Calculate percentages
    int totalGames = userGameStats.Wins + userGameStats.Losses + userGameStats.Ties;

    double winPct = totalGames > 0 ? (double)userGameStats.Wins / totalGames * 100 : 0;
    double lossPct = totalGames > 0 ? (double)userGameStats.Losses / totalGames * 100 : 0;
    double tiePct = totalGames > 0 ? (double)userGameStats.Ties / totalGames * 100 : 0;

    string statsSummary = $"**Wins:** {userGameStats.Wins} ({winPct:F1}%)\n" +
                          $"**Losses:** {userGameStats.Losses} ({lossPct:F1}%)\n" +
                          $"**Ties:** {userGameStats.Ties} ({tiePct:F1}%)\n" +
                          $"**Net Gain:** {userGameStats.NetGain} {currency.Name}";


    var embed = new EmbedBuilder()
     .WithTitle("🎲 Dice Duel Results")
     .WithColor(Color.Gold)
     .AddField("You Rolled", $"🎲 **{playerRoll}**", true)
     .AddField("Opponent Rolled", $"🎲 **{botRoll}**")
     .AddField("Starting Balance", $"{userStartBalance} {currency.Name}", true)
     .AddField("Ending Balance", $"{balance.Amount} {currency.Name}", true)
     .AddField("Your Stats", statsSummary)
     .WithFooter(footer =>
     {
       footer.Text = resultMessage;
     })
     .WithCurrentTimestamp()
     .Build();


    // Create "Play Again" button
    // var component = CasinoButtonBuilder.BuildPlayAgainButton("diceduel");
    // await context.Interaction.FollowupAsync(embed: embed, components: component);

    await context.Interaction.FollowupAsync(embed: embed);
  }

  private int SimulatedDiceRoll(int min, int max)
  {
    var random = new Random();
    int times = random.Next(1, 25);
    int roll = 0;

    for (int i = 0; i < times; i++)
      roll += random.Next(min, max + 1);

    return (roll % (max - min + 1)) + min;

  }
  private static int CalculateBotMinRoll(int betAmount, int userBalance, int dieFaces)
  {
    if (userBalance == 0) return 1;

    double percent = (double)betAmount / userBalance;
    double factor;

    if (percent < 0.05) factor = 0.0;
    else if (percent < 0.10) factor = 0.1;
    else if (percent < 0.25) factor = 0.2;
    else if (percent < 0.50) factor = 0.3;
    else if (percent < 0.75) factor = 0.4;
    else factor = 0.5;

    int rawMin = (int)(dieFaces * factor) + 1; // Add 1 for extra tilt
    int cap = (int)(dieFaces * 0.5);           // Still capped at half
    return Math.Min(rawMin, cap);
  }
  public async Task Replay(SocketInteractionContext context)
  {
    var userId = context.User.Id;
    var guildId = context.Guild.Id;

    var userRecord = await _db.Users.FirstOrDefaultAsync(u => u.DiscordId == userId);
    if (userRecord == null)
    {
      await context.Interaction.RespondAsync("⚠️ You need to use `/daily` before playing.", ephemeral: true);
      return;
    }

    var stats = await _db.UserGameStats
        .FirstOrDefaultAsync(s => s.UserId == userRecord.Id && s.GuildId == guildId && s.GameKey == GameKey);

    if (stats == null)
    {
      await context.Interaction.RespondAsync("⚠️ No recent data found for replay.", ephemeral: true);
      return;
    }

    var replayData = new GameReplayData(stats.LastGameData);
    // log replayData for debugging
    // _logger.LogInformation("Loaded GameReplayData: {@Data}", new
    // {
    //   RawData = JsonSerializer.Serialize(stats.LastGameData),
    //   BetAmount = replayData.BetAmount,
    //   DieFaces = replayData.DieFaces
    // });

    int amount = replayData.BetAmount;

    if (amount <= 0)
    {
      await context.Interaction.RespondAsync("⚠️ Could not determine your last bet amount.", ephemeral: true);
      return;
    }

    await Run(context, amount);
  }


}
