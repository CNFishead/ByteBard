using Discord.Interactions;
using Discord.WebSocket;

public static class CommandPermissionMiddleware
{
    public static async Task<bool> CheckPermissionsAsync(SocketInteractionContext context, IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        var settings = await db.ServerSettings.FindAsync(context.Guild.Id);

        if (settings?.RestrictedCommands != null)
        {
            var commandName = (context.Interaction as Discord.WebSocket.SocketSlashCommand)?.CommandName;
            if (string.IsNullOrEmpty(commandName))
            {
                // If command name is not available, we can't check permissions
                return true;
            }
            // Check if this command is restricted
            if (settings.RestrictedCommands.TryGetValue(commandName, out var allowedRoles))
            {
                var user = (SocketGuildUser)context.User;

                // If user has any of the allowed roles, allow execution
                var hasAccess = user.Roles.Any(r => allowedRoles.Contains(r.Id));

                if (!hasAccess)
                {
                    await context.Interaction.RespondAsync(
                        "❌ You don't have permission to use this command.",
                        ephemeral: true
                    );
                    return false;
                }
            }
        }

        // If not restricted or has permission
        return true;
    }
}
