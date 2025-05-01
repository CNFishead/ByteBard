using Discord.WebSocket;

public class FormatWelcomeMessage
{
  public string Format(string template, SocketGuildUser user)
  {
    return template
        .Replace("{user}", user.Mention)
        .Replace("{mention}", user.Mention)
        .Replace("{username}", user.Username)
        .Replace("{guild}", user.Guild.Name);
  }
}