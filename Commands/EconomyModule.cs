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


  

  // [SlashCommand("addcurrencytype", "Adds a new currency type to the system.")]
  // public async Task AddCurrencyType(string name)
  // {
  //   var lowerName = name.ToLower();

  //   var exists = await _dbContext.CurrencyTypes.AnyAsync(c => c.Name.ToLower() == lowerName);
  //   if (exists)
  //   {
  //     await RespondAsync($"❌ Currency `{name}` already exists.");
  //     return;
  //   }

  //   var currency = new CurrencyType { Name = name };
  //   _dbContext.CurrencyTypes.Add(currency);
  //   await _dbContext.SaveChangesAsync();

  //   await RespondAsync($"✅ Currency `{name}` has been added.");
  // }

  // [SlashCommand("setdailycurrency", "Set which currency users will receive from the /daily command.")]
  // public async Task SetDailyCurrency(string currencyName)
  // {
  //   var guildId = Context.Guild.Id;

  //   var currency = await _dbContext.CurrencyTypes
  //       .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

  //   if (currency == null)
  //   {
  //     await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
  //     return;
  //   }

  //   var settings = await _dbContext.ServerSettings
  //       .FindAsync(guildId);

  //   if (settings == null)
  //   {
  //     settings = new ServerSettings
  //     {
  //       GuildId = guildId,
  //       DailyCurrencyId = currency.Id
  //     };
  //     _dbContext.ServerSettings.Add(settings);
  //   }
  //   else
  //   {
  //     settings.DailyCurrencyId = currency.Id;
  //   }

  //   await _dbContext.SaveChangesAsync();
  //   await RespondAsync($"✅ `{currencyName}` is now the daily currency for this server.");
  // }
  // [SlashCommand("balance", "Check your balance for a specific currency.")]
  // public async Task CheckBalance(string currencyName)
  // {
  //   var userId = Context.User.Id;
  //   var username = Context.User.Username;

  //   var userRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == userId);
  //   if (userRecord == null)
  //   {
  //     await RespondAsync("❌ You don't have a user record yet. Try using `/daily` first.");
  //     return;
  //   }

  //   var currency = await _dbContext.CurrencyTypes
  //       .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

  //   if (currency == null)
  //   {
  //     await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
  //     return;
  //   }

  //   var balance = await _dbContext.CurrencyBalances
  //       .FirstOrDefaultAsync(b => b.UserId == userRecord.Id && b.CurrencyTypeId == currency.Id);

  //   int amount = balance?.Amount ?? 0;

  //   var embed = new EmbedBuilder()
  //       .WithTitle($"💰 {currency.Name} Balance")
  //       .WithDescription($"{username}, you currently have **{amount} {currency.Name}**.")
  //       .WithColor(Color.Teal)
  //       .WithTimestamp(DateTime.UtcNow)
  //       .Build();

  //   await RespondAsync(embed: embed);
  // }
  // [SlashCommand("transfercurrency", "Transfer a currency amount to another user.")]
  // public async Task TransferCurrency(SocketUser recipient, string currencyName, int amount)
  // {
  //   var senderId = Context.User.Id;
  //   var receiverId = recipient.Id;

  //   if (senderId == receiverId)
  //   {
  //     await RespondAsync("❌ You can't transfer currency to yourself.");
  //     return;
  //   }

  //   if (amount <= 0)
  //   {
  //     await RespondAsync("❌ Transfer amount must be greater than zero.");
  //     return;
  //   }

  //   var senderRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == senderId);
  //   var receiverRecord = await _dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == receiverId);

  //   if (senderRecord == null || receiverRecord == null)
  //   {
  //     await RespondAsync("❌ Both sender and recipient must have a user record.");
  //     return;
  //   }

  //   var currency = await _dbContext.CurrencyTypes
  //       .FirstOrDefaultAsync(c => c.Name.ToLower() == currencyName.ToLower());

  //   if (currency == null)
  //   {
  //     await RespondAsync($"❌ Currency `{currencyName}` does not exist.");
  //     return;
  //   }

  //   var senderBalance = await _dbContext.CurrencyBalances
  //       .FirstOrDefaultAsync(b => b.UserId == senderRecord.Id && b.CurrencyTypeId == currency.Id);

  //   if (senderBalance == null || senderBalance.Amount < amount)
  //   {
  //     await RespondAsync("❌ You don't have enough funds to complete this transaction.");
  //     return;
  //   }

  //   var receiverBalance = await _dbContext.CurrencyBalances
  //       .FirstOrDefaultAsync(b => b.UserId == receiverRecord.Id && b.CurrencyTypeId == currency.Id);

  //   if (receiverBalance == null)
  //   {
  //     receiverBalance = new UserCurrencyBalance
  //     {
  //       UserId = receiverRecord.Id,
  //       CurrencyTypeId = currency.Id,
  //       Amount = 0
  //     };
  //     _dbContext.CurrencyBalances.Add(receiverBalance);
  //   }

  //   senderBalance.Amount -= amount;
  //   receiverBalance.Amount += amount;

  //   await _dbContext.SaveChangesAsync();

  //   try
  //   {
  //     var dmChannel = await recipient.CreateDMChannelAsync();

  //     var dmEmbed = new EmbedBuilder()
  //         .WithTitle("📥 You've Received Currency!")
  //         .WithDescription($"You received **{amount} {currency.Name}** from {Context.User.Username}.")
  //         .WithColor(Color.Gold)
  //         .WithTimestamp(DateTime.UtcNow)
  //         .Build();

  //     await dmChannel.SendMessageAsync(embed: dmEmbed);
  //   }
  //   catch (Exception ex)
  //   {
  //     _logger.LogWarning($"Failed to DM user {recipient.Username}: {ex.Message}");
  //     // You might optionally notify the sender that the DM failed.
  //   }

  //   var embed = new EmbedBuilder()
  //       .WithTitle("🔁 Currency Transfer")
  //       .WithDescription($"{Context.User.Username} sent **{amount} {currency.Name}** to {recipient.Username}.")
  //       .WithColor(Color.Green)
  //       .WithTimestamp(DateTime.UtcNow)
  //       .Build();

  //   await RespondAsync(embed: embed);
  // }

}
