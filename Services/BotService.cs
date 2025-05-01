using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

public class DiscordBotService
{
  private readonly DiscordSocketClient _client;
  private readonly CommandService _commands;
  private readonly IServiceProvider _services;
  private readonly InteractionService _interactionService;

  public DiscordBotService(DiscordSocketClient client, CommandService commands, IServiceProvider services,
        InteractionService interactionService)
  {
    _client = client;
    _commands = commands;
    _services = services;
    _interactionService = interactionService;
  }

  public async Task StartAsync()
  {

    // Hook the MessageReceived event to handle commands
    _client.MessageReceived += HandleCommandAsync;
    _client.InteractionCreated += HandleInteraction;
    _client.ButtonExecuted += HandleButtonInteraction;
    _client.UserJoined += OnUserJoinedAsync;
    _client.GuildAvailable += OnGuildAvailableAsync;
    _client.JoinedGuild += OnJoinedGuildAsync;


    // Hook the Ready event to set the playing status
    _client.Ready += OnReadyAsync;

    // Optional: You can load command modules dynamically
    await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);

    // Optional: You can load interaction modules from your assembly
    await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

    // Connect the Discord client
    var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
    await _client.LoginAsync(TokenType.Bot, token);
    await _client.StartAsync();
  }

  private async Task HandleCommandAsync(SocketMessage msg)
  {
    Console.WriteLine("HandleCommandAsync fired.");

    // Don’t process system or bot messages
    if (!(msg is SocketUserMessage message)) { Console.WriteLine("System/bot message"); return; }
    ;
    if (message.Author.IsBot) return;

    Console.WriteLine($"Message content: {message.Content}");
    // Mark where the prefix ends and the command begins
    int argPos = 0;
    // For example, we’ll use '!' as the prefix
    if (!(message.HasCharPrefix('!', ref argPos) ||
          message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
      return;

    Console.WriteLine("Prefix recognized. Attempting to execute command.");

    // Create a Command Context
    var context = new SocketCommandContext(_client, message);

    // Execute the command
    var result = await _commands.ExecuteAsync(context, argPos, _services);

    Console.WriteLine($"Command result: {result.IsSuccess}, {result.ErrorReason}");

    if (!result.IsSuccess)
    {
      // Optional: handle command errors, log them, etc.
      Console.WriteLine($"Command Error: {result.ErrorReason}");
    }
  }
  private async Task OnReadyAsync()
  {
    // Option A: Register commands to a single guild for *faster updates* (guild commands update instantly)
    // ulong guildId = 669684447704121374; // Replace with your test guild ID
    // await _interactionService.RegisterCommandsToGuildAsync(guildId, true);


    // Option B (comment out if using Option A): Register globally (takes up to an hour to update)
    await _interactionService.RegisterCommandsGloballyAsync(true);
  }
  private async Task HandleInteraction(SocketInteraction arg)
  {
    try
    {
      // Create an execution context that matches the slash command
      var ctx = new SocketInteractionContext(_client, arg);

      // Execute the interaction command
      await _interactionService.ExecuteCommandAsync(ctx, _services);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error handling interaction: {ex}");
      // Handle errors
    }
  }

  private async Task HandleButtonInteraction(SocketMessageComponent component)
  {
    try
    {
      // Expecting format: casino_<gameKey>_playagain
      if (component.Data.CustomId.StartsWith("casino_") && component.Data.CustomId.EndsWith("_playagain"))
      {
        string[] parts = component.Data.CustomId.Split('_');
        if (parts.Length < 3)
        {
          await component.RespondAsync("❌ Invalid button format.", ephemeral: true);
          return;
        }

        string gameKey = parts[1];

        using var scope = _services.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<IGameHandlerRegistry>();
        var handler = registry.GetHandler(gameKey);

        if (handler == null)
        {
          await component.RespondAsync($"❌ No handler found for game `{gameKey}`.", ephemeral: true);
          return;
        }

        var context = new SocketInteractionContext(_client, component);
        await handler.Replay(context);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error in dynamic button handler: {ex}");
      await component.RespondAsync("❌ Something went wrong with the replay feature.", ephemeral: true);
    }
  }

  private async Task OnUserJoinedAsync(SocketGuildUser user)
  {
    using var scope = _services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
    var settings = await db.ServerSettings.FindAsync(user.Guild.Id);

    var guildId = user.Guild.Id;
    var roleIds = settings?.DefaultJoinRoleIds ?? new List<ulong>();

    foreach (var roleId in roleIds)
    {
      var role = user.Guild.GetRole(roleId);
      if (role != null)
      {
        await user.AddRoleAsync(role);
      }
    }

    // Send welcome message
    if (!string.IsNullOrWhiteSpace(settings?.WelcomeMessage) && settings.WelcomeChannelId.HasValue)
    {
      var channel = user.Guild.GetTextChannel(settings.WelcomeChannelId.Value);
      if (channel != null)
      {
        var formatted = new FormatWelcomeMessage().Format(settings.WelcomeMessage, user);
        await channel.SendMessageAsync(formatted);
      }
    }
  }
  private async Task OnGuildAvailableAsync(SocketGuild guild)
  {
    await EnsureServerSettingsAsync(guild);
  }

  private async Task OnJoinedGuildAsync(SocketGuild guild)
  {
    await EnsureServerSettingsAsync(guild);
  }

  private async Task EnsureServerSettingsAsync(SocketGuild guild)
  {
    try
    {
      using var scope = _services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();

      var existing = await db.ServerSettings.FindAsync(guild.Id);

      if (existing == null)
      {
        db.ServerSettings.Add(new ServerSettings
        {
          GuildId = guild.Id,
          DailyCurrencyId = 1, // Default/fallback
          DefaultJoinRoleIds = new List<ulong>() // Optional, but for clarity
        });

        await db.SaveChangesAsync();
        Console.WriteLine($"✅ Created ServerSettings for guild: {guild.Name} ({guild.Id})");
      }
      else
      {
        // Optionally update anything if needed
        Console.WriteLine($"ℹ️ ServerSettings already exists for guild: {guild.Name} ({guild.Id})");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Failed to initialize ServerSettings for guild {guild.Name} ({guild.Id}): {ex.Message}");
    }
  }

}
