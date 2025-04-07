using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class TransferCurrencyCommand : BaseEconomyModule
  {
    public TransferCurrencyCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("transfercurrency", "Transfer a currency amount to another user.")]
    public async Task TransferCurrency(SocketUser recipient, string currencyName, int amount)
    {
      try
      {

        await DeferAsync(true);
        var guildId = Context.Guild.Id;
        var senderId = Context.User.Id;
        var receiverId = recipient.Id;

        if (senderId == receiverId)
        {
          await FollowupAsync("❌ You can't transfer currency to yourself.");
          return;
        }

        if (amount <= 0)
        {
          await FollowupAsync("❌ Transfer amount must be greater than zero.");
          return;
        }

        var senderRecord = await Db.Users.FirstOrDefaultAsync(u => u.DiscordId == senderId);
        var receiverRecord = await Db.Users.FirstOrDefaultAsync(u => u.DiscordId == receiverId);

        if (senderRecord == null || receiverRecord == null)
        {
          await FollowupAsync("❌ Both sender and recipient must have a user record.");
          return;
        }

        var currency = await Db.CurrencyTypes
            .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name.ToLower() == currencyName.ToLower());

        if (currency == null)
        {
          await FollowupAsync($"❌ Currency `{currencyName}` does not exist in this server.");
          return;
        }

        var senderBalance = await Db.CurrencyBalances
            .FirstOrDefaultAsync(b => b.UserId == senderRecord.Id && b.CurrencyTypeId == currency.Id && b.GuildId == guildId);

        if (senderBalance == null || senderBalance.Amount < amount)
        {
          await FollowupAsync("❌ You don't have enough funds to complete this transaction.");
          return;
        }

        var receiverBalance = await Db.CurrencyBalances
            .FirstOrDefaultAsync(b => b.UserId == receiverRecord.Id && b.CurrencyTypeId == currency.Id && b.GuildId == guildId);

        if (receiverBalance == null)
        {
          receiverBalance = new UserCurrencyBalance
          {
            UserId = receiverRecord.Id,
            CurrencyTypeId = currency.Id,
            GuildId = guildId,
            Amount = 0
          };
          Db.CurrencyBalances.Add(receiverBalance);
        }

        senderBalance.Amount -= amount;
        receiverBalance.Amount += amount;

        await Db.SaveChangesAsync();

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
          Logger.LogWarning($"Failed to DM user {recipient.Username}: {ex.Message}");
        }

        var embed = new EmbedBuilder()
            .WithTitle("🔁 Currency Transfer")
            .WithDescription($"{Context.User.Username} sent **{amount} {currency.Name}** to {recipient.Username}.")
            .WithColor(Color.Green)
            .WithTimestamp(DateTime.UtcNow)
            .Build();

        await FollowupAsync(embed: embed);
      }
      catch (System.Exception ex)
      {
        {
          Logger.LogError(ex, "Error in AddCurrencyCommand: {Message}", ex.Message);
          await FollowupAsync("❌ Something went wrong while processing your request.");
          throw; // Rethrow the exception to be handled by the global exception handler
        }
      }
    }
  }
}
