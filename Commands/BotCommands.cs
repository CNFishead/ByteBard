using Discord.Commands;
using System.Threading.Tasks;

public class BotCommands : ModuleBase<SocketCommandContext>
{
  [Command("testwelcome")]
  public async Task TestWelcomeAsync(ulong userId)
  {
    Console.WriteLine("Request made to testwelcome");
    // Access the current guild from Context
    var guild = Context.Guild;
    if (guild == null)
    {
      await ReplyAsync("Not in a guild context.");
      return;
    }

    // Get the user from the guild
    var user = guild.GetUser(userId);
    if (user != null)
    {
      // Manually call your welcome logic, e.g.
      // OnUserJoinedAsync(user);
      await ReplyAsync($"Simulating welcome message for {user.Mention}");
    }
    else
    {
      await ReplyAsync("User not found in this guild.");
    }
  }
}
