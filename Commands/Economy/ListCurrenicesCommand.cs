using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  [Group("economy", "Economy-related commands.")]
  public class ListCurrenciesCommand : BaseEconomyModule
  {
    public ListCurrenciesCommand(ILogger<ListCurrenciesCommand> logger, BotDbContext db) : base(logger, db)
    {
      logger.LogInformation("ListCurrenciesCommand initialized.");
    }
    [SlashCommand("listcurrencies", "List all available currency types for this server.")]
    public async Task ListAsync()
    {
      var guildId = Context.Guild.Id;

      var currencies = await Db.CurrencyTypes
          .Where(c => c.GuildId == guildId)
          .ToListAsync();

      if (!currencies.Any())
      {
        await RespondAsync("ℹ️ No currencies have been added yet in this server.");
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
  }
}
