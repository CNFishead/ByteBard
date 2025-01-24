using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public partial class RandomOrgDiceRoller : IDiceRoller
{
    private const string RandomOrgBaseUrl = "https://api.random.org/json-rpc/4/invoke";
    private ILogger<RandomOrgDiceRoller> _logger;

    public RandomOrgDiceRoller(ILogger<RandomOrgDiceRoller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse a dice expression like "2d20+2", "3d6-1", "1d10" and roll each die individually.
    /// Return the list of rolls, total, faces, and modifier.
    /// </summary>
    (List<int> rolls, int total, int faces, int modifier) IDiceRoller.Roll(string diceExpr)
    {
        // Format: [X]d[Y](+|-Z)
        // _logger.LogInformation($"RollDiceExpression: {diceExpr}");

        // Regular expression to match dice expressions (e.g., "2d20+3", "3d6-2", "1d10")
        var match = Regex.Match(diceExpr, @"^(\d*)d(\d+)([+-]\d+)?$");
        if (!match.Success)
        {
            _logger.LogWarning("Invalid dice expression format.");
            return (new List<int>(), 0, 0, 0);
        }

        // Extract number of dice, faces, and modifier
        int numDice = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        int faces = int.Parse(match.Groups[2].Value);
        int modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

        // _logger.LogInformation($"Parsed Values -> Dice: {numDice}, Faces: {faces}, Modifier: {modifier}");

        // Now do the actual rolling
        var rollList = new List<int>();
        for (int i = 0; i < numDice; i++)
        {
            // _logger.LogInformation($"Rolling die {i + 1}");
            int rollValue = RollDiceUsingApi(1, 1, faces).Result;
            // _logger.LogInformation($"Roll: {rollValue}");
            rollList.Add(rollValue);
        }

        // Sum up the dice, then apply the modifier
        int total = rollList.Sum() + modifier;

        return (rollList, total, faces, modifier);
    }

    public async Task<int> RollDiceUsingApi(int num, int min, int max)
    {
        try
        {
            using var client = new HttpClient();
            var requestId = Guid.NewGuid().ToString();
            // Construct the request payload
            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "generateIntegers",
                @params = new
                {
                    apiKey = Environment.GetEnvironmentVariable("RANDOM_ORG_API_KEY"),
                    n = num,
                    min = min,
                    max = max,
                    replacement = true,
                },
                id = requestId,
            };

            // Serialize the payload to JSON
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Make the POST request
            var response = await client.PostAsync(RandomOrgBaseUrl, content);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Parse the response JSON
            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RandomOrgResponse>(responseJson, options);
            // _logger.LogInformation($"Random.org Response: {result.Result.Random.Data[0]}");
            // Return the first number in the result
            return result.Result.Random.Data[0];
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to fetch from Random.org. Falling back to simulated roll.");
            _logger.LogWarning("Error message: " + e.Message);
            return SimulatedDiceRoll(min, max);
        }
    }

    private int SimulatedDiceRoll(int min, int max)
    {
        var random = new Random();
        // generate a random number for the amount of times the dice will bounce
        var times = random.Next(1, 25);
        int roll = 0;

        // Simulate multiple bounces
        for (int i = 0; i < times; i++)
        {
            roll += random.Next(min, max + 1);
        }

        return (roll % (max - min + 1)) + min;
    }
}
