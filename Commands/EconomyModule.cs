using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
    var guildId = Context.Guild.Id;

    // Step 0: Get Daily Currency for This Server
    var settings = await _dbContext.ServerSettings
        .Include(s => s.DailyCurrency)
        .FirstOrDefaultAsync(s => s.GuildId == guildId);

    if (settings == null)
    {
      await RespondAsync("❌ This server has not set a daily currency yet. Use `/setdailycurrency` first.");
      return;
    }

    var dailyCurrency = settings.DailyCurrency;

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
      userEconomy.StreakCount = 1;
    }

    // Step 5: Calculate Daily Reward
    int baseAmount = 100;
    double streakMultiplier = GetStreakMultiplier(userEconomy.StreakCount);
    Random rnd = new Random();
    double bonusMultiplier = 1 + (rnd.NextDouble() * 0.2); // 1.0 to 1.2
    int reward = (int)(baseAmount * streakMultiplier * bonusMultiplier);

    // Step 6: Get or create user's balance for this currency
    var balance = await _dbContext.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == userRecord.Id && b.CurrencyTypeId == dailyCurrency.Id);

    if (balance == null)
    {
      balance = new UserCurrencyBalance
      {
        UserId = userRecord.Id,
        CurrencyTypeId = dailyCurrency.Id,
        Amount = 0
      };
      _dbContext.CurrencyBalances.Add(balance);
    }

    balance.Amount += reward;
    userEconomy.LastClaimed = now;

    await _dbContext.SaveChangesAsync();

    // Step 7: Respond to the user
    var embed = new EmbedBuilder()
        .WithTitle("💰 Daily Reward Claimed!")
        .WithDescription($"You received **{reward} {dailyCurrency.Name}**!")
        .AddField("📈 Streak", $"{userEconomy.StreakCount} days", true)
        .AddField("💵 Total Balance", $"{balance.Amount} {dailyCurrency.Name}", true)
        .WithColor(Color.Gold)
        .WithTimestamp(now)
        .Build();

    await RespondAsync(embed: embed);
  }


  [SlashCommand("modifycurrency", "Modify a user's currency of a specific type.")]
  public async Task ModifyCurrency(SocketUser user, string currencyName, int amount)
  {
    var discordId = user.Id;
    var username = user.Username;

    var userRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
    if (userRecord == null)
    {
      await RespondAsync("❌ That user has no record in the database.");
      return;
    }

    var currencyType = await _dbContext.CurrencyTypes
        .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

    if (currencyType == null)
    {
      await RespondAsync($"❌ Currency type `{currencyName}` does not exist.");
      return;
    }

    var balance = await _dbContext.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == userRecord.Id && b.CurrencyTypeId == currencyType.Id);

    if (balance == null)
    {
      balance = new UserCurrencyBalance
      {
        UserId = userRecord.Id,
        CurrencyTypeId = currencyType.Id,
        Amount = 0
      };
      _dbContext.CurrencyBalances.Add(balance);
    }

    balance.Amount += amount;
    await _dbContext.SaveChangesAsync();

    await RespondAsync($"✅ {username} now has {balance.Amount} {currencyType.Name}.");
  }

  [SlashCommand("addcurrencytype", "Adds a new currency type to the system.")]
  public async Task AddCurrencyType(string name)
  {
    var lowerName = name.ToLower();

    var exists = await _dbContext.CurrencyTypes.AnyAsync(c => c.Name.ToLower() == lowerName);
    if (exists)
    {
      await RespondAsync($"❌ Currency `{name}` already exists.");
      return;
    }

    var currency = new CurrencyType { Name = name };
    _dbContext.CurrencyTypes.Add(currency);
    await _dbContext.SaveChangesAsync();

    await RespondAsync($"✅ Currency `{name}` has been added.");
  }

  [SlashCommand("setdailycurrency", "Set which currency users will receive from the /daily command.")]
  public async Task SetDailyCurrency(string currencyName)
  {
    var guildId = Context.Guild.Id;

    var currency = await _dbContext.CurrencyTypes
        .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

    if (currency == null)
    {
      await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
      return;
    }

    var settings = await _dbContext.ServerSettings
        .FindAsync(guildId);

    if (settings == null)
    {
      settings = new ServerSettings
      {
        GuildId = guildId,
        DailyCurrencyId = currency.Id
      };
      _dbContext.ServerSettings.Add(settings);
    }
    else
    {
      settings.DailyCurrencyId = currency.Id;
    }

    await _dbContext.SaveChangesAsync();
    await RespondAsync($"✅ `{currencyName}` is now the daily currency for this server.");
  }
  [SlashCommand("listcurrencies", "List all available currency types.")]
  public async Task ListCurrencies()
  {
    var currencies = await _dbContext.CurrencyTypes.ToListAsync();

    if (!currencies.Any())
    {
      await RespondAsync("ℹ️ No currencies have been added yet.");
      return;
    }

    var embed = new EmbedBuilder()
        .WithTitle("💱 Available Currencies")
        .WithColor(Color.Blue);

    foreach (var currency in currencies)
    {
      embed.AddField(currency.Name, "\u200B", true);
    }

    await RespondAsync(embed: embed.Build());
  }
  [SlashCommand("balance", "Check your balance for a specific currency.")]
  public async Task CheckBalance(string currencyName)
  {
    var userId = Context.User.Id;
    var username = Context.User.Username;

    var userRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == userId);
    if (userRecord == null)
    {
      await RespondAsync("❌ You don't have a user record yet. Try using `/daily` first.");
      return;
    }

    var currency = await _dbContext.CurrencyTypes
        .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

    if (currency == null)
    {
      await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
      return;
    }

    var balance = await _dbContext.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == userRecord.Id && b.CurrencyTypeId == currency.Id);

    int amount = balance?.Amount ?? 0;

    var embed = new EmbedBuilder()
        .WithTitle($"💰 {currency.Name} Balance")
        .WithDescription($"{username}, you currently have **{amount} {currency.Name}**.")
        .WithColor(Color.Teal)
        .WithTimestamp(DateTime.UtcNow)
        .Build();

    await RespondAsync(embed: embed);
  }
  [SlashCommand("transfercurrency", "Transfer a currency amount to another user.")]
  public async Task TransferCurrency(SocketUser recipient, string currencyName, int amount)
  {
    var senderId = Context.User.Id;
    var receiverId = recipient.Id;

    if (senderId == receiverId)
    {
      await RespondAsync("❌ You can't transfer currency to yourself.");
      return;
    }

    if (amount <= 0)
    {
      await RespondAsync("❌ Transfer amount must be greater than zero.");
      return;
    }

    var senderRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == senderId);
    var receiverRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == receiverId);

    if (senderRecord == null || receiverRecord == null)
    {
      await RespondAsync("❌ Both sender and recipient must have a user record.");
      return;
    }

    var currency = await _dbContext.CurrencyTypes
        .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

    if (currency == null)
    {
      await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
      return;
    }

    var senderBalance = await _dbContext.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == senderRecord.Id && b.CurrencyTypeId == currency.Id);

    if (senderBalance == null || senderBalance.Amount < amount)
    {
      await RespondAsync("❌ You don't have enough funds to complete this transaction.");
      return;
    }

    var receiverBalance = await _dbContext.CurrencyBalances
        .FirstOrDefaultAsync(b => b.UserId == receiverRecord.Id && b.CurrencyTypeId == currency.Id);

    if (receiverBalance == null)
    {
      receiverBalance = new UserCurrencyBalance
      {
        UserId = receiverRecord.Id,
        CurrencyTypeId = currency.Id,
        Amount = 0
      };
      _dbContext.CurrencyBalances.Add(receiverBalance);
    }

    senderBalance.Amount -= amount;
    receiverBalance.Amount += amount;

    await _dbContext.SaveChangesAsync();

    try
    {
      var dmChannel = await recipient.CreateDMChannelAsync();

      var dmEmbed = new EmbedBuilder()
          .WithTitle("📥 You've Received Currency!")
          .WithDescription($"You received **{amount} {currency.Name}** from {Context.User.Username}.")
          .WithColor(Color.Gold)
          .WithTimestamp(DateTime.UtcNow)
          .Build();

      await dmChannel.SendMessageAsync(embed: dmEmbed);
    }
    catch (Exception ex)
    {
      _logger.LogWarning($"Failed to DM user {recipient.Username}: {ex.Message}");
      // You might optionally notify the sender that the DM failed.
    }

    var embed = new EmbedBuilder()
        .WithTitle("🔁 Currency Transfer")
        .WithDescription($"{Context.User.Username} sent **{amount} {currency.Name}** to {recipient.Username}.")
        .WithColor(Color.Green)
        .WithTimestamp(DateTime.UtcNow)
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
