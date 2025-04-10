using System.Text.Json;

public class GameReplayData
{
  private readonly Dictionary<string, JsonElement> _data;

  public GameReplayData(Dictionary<string, JsonElement> data)
  {
    _data = data;
  }

  public int BetAmount => Get<int>("amount", 0);
  public string Choice => Get<string>("choice", "Heads");
  public int DieFaces => Get<int>("dieFaces", 6);

  private T Get<T>(string key, T fallback)
  {
    if (_data.TryGetValue(key, out var json))
    {
      try
      {
        var raw = json.ToString(); // Reserialize to raw JSON string
        return JsonSerializer.Deserialize<T>(raw)!;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[GameReplayData] Error parsing key '{key}': {ex.Message}");
      }
    }

    return fallback;
  }

}
