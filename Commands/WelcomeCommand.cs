using Discord;
using Discord.Interactions;
using System.Text;
using System.Threading.Tasks;

public class WelcomeModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("welcome", "Welcome a new user to the server")]
    public async Task WelcomeUser(
        [Summary("user", "The user to welcome")] IUser user)
    {
        // incase the command takes too long to respond, defer
        await DeferAsync();
        // Static channel/role/user IDs (replace these with your actual IDs)
        const ulong AboutUsChannelId = 1134609148487675914;
        const ulong SettingsChannelId = 1299898223762083891;
        const ulong WorldMasterRoleId = 1137078504085803179;
        const ulong MephistophalesId = 236633193128787968;
        const ulong NovaId = 479121165609074689;
        const ulong SeraId = 455143115439734784;
        const ulong CnfishId = 669683901513334834;
        

        // Fetch guild, channels, roles, and users dynamically
        var guild = Context.Guild;
        var aboutUsChannel = guild.GetTextChannel(AboutUsChannelId);
        var settingsChannel = guild.GetTextChannel(SettingsChannelId);
        var worldMasterRole = guild.GetRole(WorldMasterRoleId);
        var mephistophales = guild.GetUser(MephistophalesId);
        var novaId = guild.GetUser(NovaId);
        var seraId = guild.GetUser(SeraId);
        var cnfishId = guild.GetUser(CnfishId);

        // Build the response dynamically
        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine($"Welcome, {user.Mention}! We're glad you're here.");
        responseBuilder.AppendLine($"Take a look around the {aboutUsChannel.Mention} channel, and check out {settingsChannel.Mention} to get started.");
        responseBuilder.AppendLine($"If you have any questions, please reach out to a {worldMasterRole.Mention} such as {mephistophales.Mention}, {novaId.Mention}, {seraId.Mention}, or {cnfishId.Mention}.");

        // Send the response
        await FollowupAsync(responseBuilder.ToString());
    }
}
