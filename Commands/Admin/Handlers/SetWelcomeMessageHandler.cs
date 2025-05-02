using Discord;
using Discord.Interactions;

public class SetWelcomeMessageHandler
{
  private readonly ILogger<SetWelcomeMessageHandler> _logger;
  private readonly BotDbContext _db;

  public SetWelcomeMessageHandler(ILogger<SetWelcomeMessageHandler> logger, BotDbContext db)
  {
    _logger = logger;
    _db = db;
  }

  public async Task SetMessage(SocketInteractionContext context, string message)
  {
    await context.Interaction.DeferAsync(ephemeral: true);

    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);
    if (settings == null)
    {
      settings = new ServerSettings
      {
        GuildId = context.Guild.Id,
        DailyCurrencyId = 1 // fallback or ensure it's set later
      };
      _db.ServerSettings.Add(settings);
    }

    settings.WelcomeMessage = message;
    await _db.SaveChangesAsync();

    await context.Interaction.FollowupAsync("✅ Welcome message updated.");
  }

  public async Task SetChannel(SocketInteractionContext context, ITextChannel channel)
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

    settings.WelcomeChannelId = channel.Id;
    await _db.SaveChangesAsync();

    await context.Interaction.FollowupAsync($"📢 Welcome messages will be sent in {channel.Mention}.");
  }

  public async Task Show(SocketInteractionContext context)
  {
    await context.Interaction.DeferAsync(ephemeral: true);

    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings == null || (string.IsNullOrWhiteSpace(settings.WelcomeMessage) && !settings.WelcomeChannelId.HasValue))
    {
      await context.Interaction.FollowupAsync("⚠️ No welcome message or channel configured.");
      return;
    }

    var preview = settings.WelcomeMessage ?? "*No message set*";
    var channelMention = settings.WelcomeChannelId.HasValue
        ? $"<#{settings.WelcomeChannelId}>"
        : "*No channel set*";

    await context.Interaction.FollowupAsync($"👋 **Welcome Settings**\nMessage: `{preview}`\nChannel: {channelMention}");
  }
}
