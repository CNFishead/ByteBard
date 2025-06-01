using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class RestrictCommandHandler
{
  private readonly ILogger<RestrictCommandHandler> _logger;
  private readonly BotDbContext _db;

  public RestrictCommandHandler(ILogger<RestrictCommandHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }
  [DefaultMemberPermissions(GuildPermission.Administrator)]
  public async Task Restrict(SocketInteractionContext context, string commandName, IRole role)
  {
    if (!IsAdmin(context))
    {
      await context.Interaction.RespondAsync("❌ You must be a server admin to use this command.", ephemeral: true);
      return;
    }

    await context.Interaction.DeferAsync(ephemeral: true);
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id)
        ?? new ServerSettings { GuildId = context.Guild.Id, DailyCurrencyId = 1 };

    settings.RestrictedCommands ??= new();
    if (!settings.RestrictedCommands.TryGetValue(commandName, out var roles))
    {
      roles = new List<ulong>();
      settings.RestrictedCommands[commandName] = roles;
    }

    if (roles.Contains(role.Id))
    {
      await context.Interaction.FollowupAsync("⚠️ That role already has access to this command.");
      return;
    }

    roles.Add(role.Id);
    _db.Entry(settings).Property(s => s.RestrictedCommands).IsModified = true;
    await _db.SaveChangesAsync();

    await context.Interaction.FollowupAsync($"✅ `{role.Name}` can now use `{commandName}`.");
  }
  [DefaultMemberPermissions(GuildPermission.Administrator)]
  public async Task Unrestrict(SocketInteractionContext context, string commandName, IRole role)
  {
    if (!IsAdmin(context))
    {
      await context.Interaction.RespondAsync("❌ You must be a server admin to use this command.", ephemeral: true);
      return;
    }

    await context.Interaction.DeferAsync(ephemeral: true);
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings?.RestrictedCommands == null ||
        !settings.RestrictedCommands.TryGetValue(commandName, out var roles) ||
        !roles.Contains(role.Id))
    {
      await context.Interaction.FollowupAsync("⚠️ That role is not currently restricted for this command.");
      return;
    }

    roles.Remove(role.Id);
    if (roles.Count == 0) settings.RestrictedCommands.Remove(commandName);

    _db.Entry(settings).Property(s => s.RestrictedCommands).IsModified = true;
    await _db.SaveChangesAsync();

    await context.Interaction.FollowupAsync($"🗑️ `{role.Name}` removed from `{commandName}` restrictions.");
  }
  [DefaultMemberPermissions(GuildPermission.Administrator)]
  public async Task List(SocketInteractionContext context)
  {
    if (!IsAdmin(context))
    {
      await context.Interaction.RespondAsync("❌ You must be a server admin to use this command.", ephemeral: true);
      return;
    }

    await context.Interaction.DeferAsync(ephemeral: true);
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings?.RestrictedCommands == null || settings.RestrictedCommands.Count == 0)
    {
      await context.Interaction.FollowupAsync("📭 No command restrictions are currently set.");
      return;
    }

    var lines = settings.RestrictedCommands.Select(kvp =>
    {
      var roles = kvp.Value
              .Select(id => context.Guild.GetRole(id))
              .Where(r => r != null)
              .Select(r => r!.Mention);

      return $"`/{kvp.Key}` → {string.Join(", ", roles)}";
    });

    await context.Interaction.FollowupAsync("🔒 Command Restrictions:\n" + string.Join("\n", lines));
  }

  private bool IsAdmin(SocketInteractionContext context)
  {
    var user = context.User as SocketGuildUser;
    return user?.GuildPermissions.Administrator == true || user.Id == context.Guild.OwnerId;
  }

}
