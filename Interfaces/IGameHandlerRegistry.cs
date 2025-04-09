public interface IGameHandlerRegistry
{
    IGameHandler? GetHandler(string gameKey);
}
public class GameHandlerRegistry : IGameHandlerRegistry
{
    private readonly Dictionary<string, IGameHandler> _handlers;

    public GameHandlerRegistry(IEnumerable<IGameHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.GameKey);
    }

    public IGameHandler? GetHandler(string gameKey)
    {
        return _handlers.TryGetValue(gameKey, out var handler) ? handler : null;
    }
}
