using System.Text;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class DiceDuelHandler
{
  private readonly ILogger _logger;
  private readonly BotDbContext _db;

  public DiceDuelHandler(ILogger logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  public async Task Run(SocketInteractionContext context, int amount)
  {

    var userId = context.User.Id;
    var guildId = context.Guild.Id;

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

    int playerRoll = SimulatedDiceRoll(1, 6);
    int botRoll = SimulatedDiceRoll(1, 6);

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

    await _db.SaveChangesAsync();

    var output = new StringBuilder();
    output.AppendLine("🎲 Rolling dice...");
    output.AppendLine($"You: 🎲 **{playerRoll}**");
    output.AppendLine($"Opponent: 🎲 **{botRoll}**");
    output.AppendLine(resultMessage);

    await context.Interaction.FollowupAsync(output.ToString());
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
}
