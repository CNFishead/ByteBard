using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class EconomyModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly ILogger _logger;
  private readonly BotDbContext _dbContext;

  public EconomyModule(ILogger<EconomyModule> logger, BotDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  [SlashCommand("daily", "Claim your daily reward!")]
  public async Task DailyCommand()
  {
    _logger.LogInformation("DailyCommand fired.");
    var discordId = Context.User.Id;
    var username = Context.User.Username;

    // Step 1: Ensure UserRecord Exists
    var userRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
    if (userRecord == null)
    {
      _logger.LogInformation($"Creating new user record for {username} ({discordId})");
      userRecord = new UserRecord
      {
        DiscordId = discordId,
        Username = username
      };
      _dbContext.Users.Add(userRecord);
      await _dbContext.SaveChangesAsync();
      _logger.LogInformation($"Created new user record for {username} ({discordId})");
    }

    // Step 2: Ensure UserEconomy Exists
    var userEconomy = await _dbContext.Economies.FindAsync(userRecord.Id);
    if (userEconomy == null)
    {
      _logger.LogInformation($"Creating new economy record for {username} ({discordId})");
      userEconomy = new UserEconomy
      {
        UserId = userRecord.Id,
        User = userRecord
      };
      _dbContext.Economies.Add(userEconomy);
      await _dbContext.SaveChangesAsync();
      _logger.LogInformation($"Created new economy record for {username} ({discordId})");
    }

    // Step 3: Check if user has already claimed today
    var now = DateTime.UtcNow;
    if (userEconomy.LastClaimed.HasValue && userEconomy.LastClaimed.Value.Date == now.Date)
    {
      _logger.LogInformation($"User {username} ({discordId}) has already claimed their daily reward today.");
      await RespondAsync("❌ You've already claimed your daily reward today! Try again tomorrow.");
      return;
    }

    // Step 4: Streak Handling
    if (userEconomy.LastClaimed.HasValue && userEconomy.LastClaimed.Value.Date == now.Date.AddDays(-1))
    {
      userEconomy.StreakCount++;
    }
    else
    {
      userEconomy.StreakCount = 1; // Reset streak if they missed a day
    }

    // Step 5: Calculate Daily Reward
    int baseAmount = 100;
    double streakMultiplier = GetStreakMultiplier(userEconomy.StreakCount);
    Random rnd = new Random();
    double bonusMultiplier = 1 + (rnd.NextDouble() * 0.2); // 1.0 to 1.2 random range
    int reward = (int)(baseAmount * streakMultiplier * bonusMultiplier);

    userEconomy.CurrencyAmount += reward;
    userEconomy.LastClaimed = now;

    await _dbContext.SaveChangesAsync();

    // Step 6: Respond to the user
    var embed = new EmbedBuilder()
        .WithTitle("💰 Daily Reward Claimed!")
        .WithDescription($"You received **{reward}** credits!")
        .AddField("📈 Streak", $"{userEconomy.StreakCount} days", true)
        .AddField("💵 Total Balance", $"{userEconomy.CurrencyAmount} credits", true)
        .WithColor(Color.Gold)
        .WithTimestamp(now)
        .Build();

    await RespondAsync(embed: embed);
  }

  private double GetStreakMultiplier(int streak)
  {
    if (streak >= 14) return 1.0;
    if (streak >= 7) return 0.75;
    if (streak >= 4) return 0.5;
    return 0.25;
  }
}
