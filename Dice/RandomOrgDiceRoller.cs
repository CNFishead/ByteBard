using System.Text;
using System.Text.Json;

public partial class RandomOrgDiceRoller : IDiceRoller
{
    private const string RandomOrgBaseUrl = "https://api.random.org/json-rpc/4/invoke";
    private ILogger<RandomOrgDiceRoller> _logger;


    // Constructor taking an ILogger
    public RandomOrgDiceRoller(ILogger<RandomOrgDiceRoller> logger)
    {
        _logger = logger;

    }

    /// <summary>
    /// Parse a dice expression like "2d20+2" and roll each die individually.
    /// Return the list of rolls, total, faces, and modifier.
    /// </summary>
    (List<int> rolls, int total, int faces, int modifier) IDiceRoller.Roll(string diceExpr)
    {
        // Format: [X]d[Y](+Z)
        // e.g. "2d20+2" => 2 dice, 20 faces, +2
        // We'll parse it in a naive way: split on 'd', then check for '+'
        _logger.LogInformation($"RollDiceExpression: {diceExpr}");
        var parts = diceExpr.Split('d');
        if (parts.Length != 2)
        {
            // invalid format
            return (new List<int>(), 0, 0, 0);
        }

        int numDice = 1;
        int.TryParse(parts[0], out numDice);

        int faces = 20;
        int modifier = 0;

        var plusIndex = parts[1].IndexOf('+');
        if (plusIndex >= 0)
        {
            // e.g. "20+2"
            var facePart = parts[1].Substring(0, plusIndex);
            var modPart = parts[1].Substring(plusIndex + 1);
            int.TryParse(facePart, out faces);
            int.TryParse(modPart, out modifier);
        }
        else
        {
            int.TryParse(parts[1], out faces);
        }

        // Now do the actual rolling
        var random = new Random();
        var rollList = new List<int>();
        for (int i = 0; i < numDice; i++)
        {
            _logger.LogInformation($"Rolling die {i + 1}");
            int rollValue = RollDiceUsingApi(1, 1, faces).Result;
            _logger.LogInformation($"Roll: {rollValue}");
            rollList.Add(rollValue);
        }

        // Sum up the dice, then add the modifier
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

            // Return the first number in the result
            return result.Result.Random.Data[0];
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to fetch from Random.org. Falling back to simulated roll.");
            // console log the error message from the api
            _logger.LogWarning("Error message: " + e.Message);
            // Fallback to simulated dice roll
            return SimulatedDiceRoll(min, max);
        }
    }

    private int SimulatedDiceRoll(int min, int max)
    {
        var random = new Random();
        // generate a random number for the amount of times the dice will bounce
        var times = random.Next(1, 25); // 1 to 25 times
        int roll = 0;

        // Simulate multiple bounces
        for (int i = 0; i < times; i++)
        {
            roll += random.Next(min, max + 1);
        }

        return (roll % (max - min + 1)) + min;
    }
}
