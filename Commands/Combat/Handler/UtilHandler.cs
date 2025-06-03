using Discord.Interactions;
using Discord.WebSocket; 
using Microsoft.EntityFrameworkCore;

namespace FallVerseBotV2.Commands.Combat;

public static class TrackerUtils
{
    public static bool HasAdminPermissions(SocketInteractionContext context)
    {
        if (context.User is not SocketGuildUser guildUser)
            return false;

        return guildUser.GuildPermissions.Administrator;
    }

    public static async Task<CombatTracker?> TryGetTrackerAsync(
        BotDbContext db,
        SocketInteractionContext context,
        string gameId,
        bool includeCombatants = true)
    {
        var guildId = context.Guild.Id;
        var channelId = context.Channel.Id;

        var query = db.CombatTrackers.AsQueryable();

        if (includeCombatants)
        {
            query = query.Include(t => t.Combatants);
        }

        return await query.FirstOrDefaultAsync(t =>
            t.GuildId == guildId &&
            t.ChannelId == channelId &&
            t.GameId == gameId);
    }

    public static bool IsTrackerOwnerOrAdmin(CombatTracker tracker, SocketInteractionContext context)
    {
        return tracker.CreatedByUserId == context.User.Id || HasAdminPermissions(context);
    }
}
