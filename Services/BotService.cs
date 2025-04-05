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
    if (!(msg is SocketUserMessage message)) { Console.WriteLine("System/bot message"); return; };
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
    // await _interactionService.RegisterCommandsToGuildAsync(guildId);

    // Option B (comment out if using Option A): Register globally (takes up to an hour to update)
    await _interactionService.RegisterCommandsGloballyAsync();
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
}
