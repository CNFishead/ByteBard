using Discord.Interactions;

public interface IGameHandler
{
    string GameKey { get; }
    Task Run(SocketInteractionContext context, int amount);
    Task Replay(SocketInteractionContext context);
}
