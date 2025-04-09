using Discord.Interactions;

public interface IGameHandler
{
    string GameKey { get; } 
    Task Replay(SocketInteractionContext context);
}
