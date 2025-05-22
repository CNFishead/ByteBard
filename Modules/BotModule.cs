using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

public class BotModule : IBaseModule
{
    public void RegisterServices(IServiceCollection services)
    {
        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildMembers
        };

        // Register the DiscordSocketClient with custom config
        services.AddSingleton(new DiscordSocketClient(socketConfig));

        // Register the CommandService (for text commands)
        services.AddSingleton<CommandService>();

        // Register the InteractionService (for slash commands) explicitly
        services.AddSingleton<InteractionService>(sp =>
        {
            var client = sp.GetRequiredService<DiscordSocketClient>();
            // Optional config for slash commands
            var interactionConfig = new InteractionServiceConfig
            {
                // Example settings
                DefaultRunMode = Discord.Interactions.RunMode.Async
            };
            return new InteractionService(client, interactionConfig);
        });
        services.AddSingleton<IDiceRoller, RandomOrgDiceRoller>();
        // Finally, register your DiscordBotService
        services.AddSingleton<DiscordBotService>();


        // Register game handlers and registry
        services.AddScoped<IGameHandler, DiceDuelHandler>(); // Add other handlers similarly
        services.AddScoped<IGameHandler, CoinFlipHandler>();
        services.AddScoped<IGameHandlerRegistry, GameHandlerRegistry>();
    }
}
