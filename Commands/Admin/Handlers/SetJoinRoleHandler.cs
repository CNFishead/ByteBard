using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;


public class SetJoinRoleHandler
{
  private readonly ILogger<SetJoinRoleHandler> _logger;
  private readonly BotDbContext _db;

  public SetJoinRoleHandler(ILogger<SetJoinRoleHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  public async Task Add(SocketInteractionContext context, IRole role)
  {
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);
    if (settings == null)
    {
      settings = new ServerSettings
      {
        GuildId = context.Guild.Id,
        DailyCurrencyId = 1 // fallback, replace with proper default
      };
      _db.ServerSettings.Add(settings);
    }

    if (settings.DefaultJoinRoleIds.Contains(role.Id))
    {
      await context.Interaction.RespondAsync("⚠️ That role is already assigned to new users.", ephemeral: true);
      return;
    }

    settings.DefaultJoinRoleIds.Add(role.Id);
    await _db.SaveChangesAsync();

    await context.Interaction.RespondAsync($"✅ `{role.Name}` will now be auto-assigned to new users.", ephemeral: true);
  }

  public async Task Remove(SocketInteractionContext context, IRole role)
  {
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);
    if (settings == null || !settings.DefaultJoinRoleIds.Contains(role.Id))
    {
      await context.Interaction.RespondAsync("⚠️ That role isn’t currently set as a default join role.", ephemeral: true);
      return;
    }

    settings.DefaultJoinRoleIds.Remove(role.Id);
    await _db.SaveChangesAsync();

    await context.Interaction.RespondAsync($"🗑️ `{role.Name}` removed from default join roles.", ephemeral: true);
  }

  public async Task List(SocketInteractionContext context)
  {
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings == null || settings.DefaultJoinRoleIds.Count == 0)
    {
      await context.Interaction.RespondAsync("📭 No default join roles set.", ephemeral: true);
      return;
    }

    var mentions = settings.DefaultJoinRoleIds
        .Select(id => context.Guild.GetRole(id))
        .Where(r => r != null)
        .Select(r => r!.Mention);

    await context.Interaction.RespondAsync("📌 Default join roles:\n" + string.Join("\n", mentions), ephemeral: true);
  }

}
