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

    await context.Interaction.RespondAsync("✅ Welcome message updated.", ephemeral: true);
  }

  public async Task SetChannel(SocketInteractionContext context, ITextChannel channel)
  {
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

    await context.Interaction.RespondAsync($"📢 Welcome messages will be sent in {channel.Mention}.", ephemeral: true);
  }

  public async Task Show(SocketInteractionContext context)
  {
    var settings = await _db.ServerSettings.FindAsync(context.Guild.Id);

    if (settings == null || (string.IsNullOrWhiteSpace(settings.WelcomeMessage) && !settings.WelcomeChannelId.HasValue))
    {
      await context.Interaction.RespondAsync("⚠️ No welcome message or channel configured.", ephemeral: true);
      return;
    }

    var preview = settings.WelcomeMessage ?? "*No message set*";
    var channelMention = settings.WelcomeChannelId.HasValue
        ? $"<#{settings.WelcomeChannelId}>"
        : "*No channel set*";

    await context.Interaction.RespondAsync($"👋 **Welcome Settings**\nMessage: `{preview}`\nChannel: {channelMention}", ephemeral: true);
  }
}
