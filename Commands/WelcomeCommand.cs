using Discord;
using Discord.Interactions;
using System.Text;
using System.Threading.Tasks;

public class WelcomeModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger;

    public WelcomeModule(ILogger<WelcomeModule> logger)
    {
        _logger = logger;
        _logger.LogInformation("WelcomeModule constructed");
    }

    [SlashCommand("welcome", "Welcome a new user to the server")]
    public async Task WelcomeUser(
        [Summary("user", "The user to welcome")] IUser user)
    {
        await DeferAsync();
        try
        {
            _logger.LogInformation("WelcomeUser fired.");

            // Static channel/role/user IDs (replace these with actual IDs)
            const ulong AboutUsChannelId = 1134609148487675914;
            const ulong SettingsChannelId = 1299898223762083891;
            const ulong WorldMasterRoleId = 1137078504085803179;
            const ulong MephistophalesId = 236633193128787968;
            const ulong NovaId = 479121165609074689;
            const ulong SeraId = 455143115439734784;
            const ulong CnfishId = 669683901513334834;
            const ulong SkyeId = 290651318262169600;

            // Fetch guild, channels, roles, and users dynamically
            var guild = Context.Guild;
            if (guild == null)
            {
                await FollowupAsync("This command must be run in a server.");
                return;
            }

            var aboutUsChannel = guild.GetTextChannel(AboutUsChannelId);
            var settingsChannel = guild.GetTextChannel(SettingsChannelId);
            var worldMasterRole = guild.GetRole(WorldMasterRoleId);
            // Fetch all users in the guild
            var users = await guild.GetUsersAsync().FlattenAsync();
            // Find specific users
            var mephistophales = users.FirstOrDefault(u => u.Id == MephistophalesId);
            var novaId = users.FirstOrDefault(u => u.Id == NovaId);
            var seraId = users.FirstOrDefault(u => u.Id == SeraId);
            var cnfishId = users.FirstOrDefault(u => u.Id == CnfishId);
            var skyeId = users.FirstOrDefault(u => u.Id == SkyeId);

            _logger.LogInformation($"Fetched guild, channels, roles, and users.");

            // Build the response dynamically with null checks
            var responseBuilder = new StringBuilder();
            responseBuilder.AppendLine($"Welcome, {user.Mention}! We're glad you're here.");
            responseBuilder.AppendLine($"Take a look around the {(aboutUsChannel != null ? aboutUsChannel.Mention : "#about-us")} channel, and check out {(settingsChannel != null ? settingsChannel.Mention : "#fallverse-settings")} to get started.");

            responseBuilder.AppendLine($"If you have any questions, please reach out to a {(worldMasterRole != null ? worldMasterRole.Mention : "@World Master")} such as " +
                $"{(mephistophales != null ? mephistophales.Mention : "@Mephistophales")}, " +
                $"{(novaId != null ? novaId.Mention : "@Nova")}, " +
                $"{(seraId != null ? seraId.Mention : "@Sera")}, " +
                $"{(skyeId != null ? skyeId.Mention : "@skyeId")}, or " +
                $"{(cnfishId != null ? cnfishId.Mention : "@Cnfish")}.");

            _logger.LogInformation($"Response built.");

            // Send the response
            await FollowupAsync(responseBuilder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WelcomeUser");
            await FollowupAsync("An error occurred while welcoming the user.");
        }

    }
}
