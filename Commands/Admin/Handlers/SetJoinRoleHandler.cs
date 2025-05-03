using Discord;
using Discord.Interactions;

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
    await context.Interaction.DeferAsync(ephemeral: true);

    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);
    if (settings == null)
    {
      settings = new ServerSettings
      {
        GuildId = context.Guild.Id,
        DailyCurrencyId = 1
      };
      _db.ServerSettings.Add(settings);
    }

    settings.DefaultJoinRoleIds ??= new List<ulong>();

    if (settings.DefaultJoinRoleIds.Contains(role.Id))
    {
      await context.Interaction.FollowupAsync("⚠️ That role is already assigned to new users.");
      return;
    }

    settings.DefaultJoinRoleIds.Add(role.Id);
    _db.Entry(settings).Property(s => s.DefaultJoinRoleIds).IsModified = true;
    await _db.SaveChangesAsync();


    await context.Interaction.FollowupAsync($"✅ `{role.Name}` will now be auto-assigned to new users.");
  }

  public async Task Remove(SocketInteractionContext context, IRole role)
  {
    await context.Interaction.DeferAsync(ephemeral: true);

    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings?.DefaultJoinRoleIds == null || !settings.DefaultJoinRoleIds.Contains(role.Id))
    {
      await context.Interaction.FollowupAsync("⚠️ That role isn’t currently set as a default join role.");
      return;
    }

    settings.DefaultJoinRoleIds.Remove(role.Id);
    _db.Entry(settings).Property(s => s.DefaultJoinRoleIds).IsModified = true;
    await _db.SaveChangesAsync();


    await context.Interaction.FollowupAsync($"🗑️ `{role.Name}` removed from default join roles.");
  }

  public async Task List(SocketInteractionContext context)
  {
    await context.Interaction.DeferAsync(ephemeral: true);

    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings?.DefaultJoinRoleIds == null || settings.DefaultJoinRoleIds.Count == 0)
    {
      await context.Interaction.FollowupAsync("📭 No default join roles set.");
      return;
    }

    var mentions = settings.DefaultJoinRoleIds
        .Select(id => context.Guild.GetRole(id))
        .Where(r => r != null)
        .Select(r => r!.Mention);

    await context.Interaction.FollowupAsync("📌 Default join roles:\n" + string.Join("\n", mentions));
  }
}
