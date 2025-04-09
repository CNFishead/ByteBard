using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Economy
{
  public class SetDailyStreakCommand : BaseEconomyModule
  {
    public SetDailyStreakCommand(ILogger<BaseEconomyModule> logger, BotDbContext db) : base(logger, db) { }

    [SlashCommand("setstreak", "Manually set a user's daily streak count.")]
    public async Task SetStreak(SocketUser user, int streak)
    {
      try
      {
        await DeferAsync(true);

        var guildId = Context.Guild.Id;
        var discordId = user.Id;

        // Find user record
        var userRecord = await Db.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);
        if (userRecord == null)
        {
          await FollowupAsync("❌ That user has no record in the database.");
          return;
        }

        // Find or create UserEconomy row
        var economy = await Db.Economies
            .FirstOrDefaultAsync(e => e.UserId == userRecord.Id && e.GuildId == guildId);

        if (economy == null)
        {
          economy = new UserEconomy
          {
            UserId = userRecord.Id,
            GuildId = guildId,
            StreakCount = streak
          };
          Db.Economies.Add(economy);
        }
        else
        {
          economy.StreakCount = streak;
        }

        await Db.SaveChangesAsync();
        await FollowupAsync($"✅ {user.Mention}'s daily streak has been set to `{streak}`.");
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Error in SetStreakCommand: {Message}", ex.Message);
        await FollowupAsync("❌ Something went wrong while processing your request.");
        throw;
      }
    }
  }
}
