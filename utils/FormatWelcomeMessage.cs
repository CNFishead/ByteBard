using System.Text;
using Discord;
using Discord.WebSocket;

public class FormatWelcomeMessage
{
  public string Format(string template, SocketGuildUser user, IUser? requester = null)
  {
    var replaced = template
        .Replace("{user}", user.Mention)
        .Replace("{mention}", user.Mention)
        .Replace("{username}", user.Username)
        .Replace("{guild}", user.Guild.Name);

    // Handle requester info safely
    if (requester != null)
    {
      replaced = replaced
          .Replace("{requester}", requester.Mention)
          .Replace("{requestername}", requester.Username)
          .Replace("{requestertag}", requester.ToString());
    }
    else
    {
      replaced = replaced
          .Replace("{requester}", "*The system*")
          .Replace("{requestername}", "System")
          .Replace("{requestertag}", "System");
    }


    // Convert escaped "\n" into actual newlines
    var lines = replaced.Split("\\n", StringSplitOptions.None);

    var builder = new StringBuilder();
    foreach (var line in lines)
    {
      builder.AppendLine(line.Trim());
    }

    return builder.ToString();
  }
}