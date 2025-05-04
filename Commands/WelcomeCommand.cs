using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;

public class WelcomeModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger;
    private readonly BotDbContext _db;

    public WelcomeModule(ILogger<WelcomeModule> logger, BotDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [SlashCommand("welcome", "Manually welcome a user using the server's custom message")]
    public async Task WelcomeUser([Summary("user", "The user to welcome")] SocketGuildUser user)
    {
        await DeferAsync(); // Optionally use ephemeral: false if you want visible follow-up

        var guild = Context.Guild;
        if (guild == null)
        {
            await FollowupAsync("⚠️ This command must be run in a server.");
            return;
        }

        var settings = await _db.ServerSettings.FindAsync(guild.Id);
        if (settings == null || string.IsNullOrWhiteSpace(settings.ManualWelcomeMessage))
        {
            await FollowupAsync("⚠️ No manual welcome message has been configured for this server.");
            return;
        }

        var formattedMessage = new FormatWelcomeMessage().Format(settings.ManualWelcomeMessage, user);

        // Send the message to the channel where the command was invoked
        await FollowupAsync(formattedMessage); // Public by default unless otherwise specified
    }
   
}
