using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Data;

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

    /// <summary>
    /// Advanced dice rolling with support for PEMDAS operations and best/worst keeping
    /// Supports expressions like: 3d6x2+1, 4d6b3, 4d6w3, 2d20+5x3-2
    /// </summary>
    public (List<int> allRolls, List<int> keptRolls, int total, string expression) RollAdvanced(string diceExpr)
    {
        try
        {
            _logger.LogInformation($"RollAdvanced: {diceExpr}");

            // Parse the dice expression for keep best/worst modifiers
            var (cleanExpr, keepBest, keepWorst) = ParseKeepModifiers(diceExpr);

            // Parse basic dice notation
            var diceMatch = Regex.Match(cleanExpr, @"(\d*)d(\d+)");
            if (!diceMatch.Success)
            {
                _logger.LogWarning("Invalid dice expression format.");
                return (new List<int>(), new List<int>(), 0, diceExpr);
            }

            int numDice = string.IsNullOrEmpty(diceMatch.Groups[1].Value) ? 1 : int.Parse(diceMatch.Groups[1].Value);
            int faces = int.Parse(diceMatch.Groups[2].Value);

            // Roll all dice
            var allRolls = new List<int>();
            for (int i = 0; i < numDice; i++)
            {
                int rollValue = RollDiceUsingApi(1, 1, faces).Result;
                allRolls.Add(rollValue);
            }

            // Apply keep best/worst logic
            var keptRolls = ApplyKeepLogic(allRolls, keepBest, keepWorst);

            // Replace the dice expression with the sum of kept rolls in the mathematical expression
            var mathExpression = cleanExpr.Replace(diceMatch.Value, keptRolls.Sum().ToString());

            // Evaluate the mathematical expression using PEMDAS
            var total = EvaluateMathExpression(mathExpression);

            return (allRolls, keptRolls, total, diceExpr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in RollAdvanced for expression: {diceExpr}");
            return (new List<int>(), new List<int>(), 0, diceExpr);
        }
    }

    /// <summary>
    /// Parse keep best/worst modifiers from dice expression
    /// Examples: 4d6b3 -> keep best 3, 4d6w2 -> keep worst 2
    /// </summary>
    private (string cleanExpr, int? keepBest, int? keepWorst) ParseKeepModifiers(string diceExpr)
    {
        int? keepBest = null;
        int? keepWorst = null;
        string cleanExpr = diceExpr;

        // Match keep best pattern (e.g., "4d6b3")
        var bestMatch = Regex.Match(diceExpr, @"(\d*d\d+)b(\d+)");
        if (bestMatch.Success)
        {
            keepBest = int.Parse(bestMatch.Groups[2].Value);
            cleanExpr = diceExpr.Replace(bestMatch.Value, bestMatch.Groups[1].Value);
        }

        // Match keep worst pattern (e.g., "4d6w3")
        var worstMatch = Regex.Match(diceExpr, @"(\d*d\d+)w(\d+)");
        if (worstMatch.Success)
        {
            keepWorst = int.Parse(worstMatch.Groups[2].Value);
            cleanExpr = diceExpr.Replace(worstMatch.Value, worstMatch.Groups[1].Value);
        }

        return (cleanExpr, keepBest, keepWorst);
    }

    /// <summary>
    /// Apply keep best/worst logic to dice rolls
    /// </summary>
    private List<int> ApplyKeepLogic(List<int> allRolls, int? keepBest, int? keepWorst)
    {
        if (keepBest.HasValue)
        {
            return allRolls.OrderByDescending(x => x).Take(keepBest.Value).ToList();
        }
        else if (keepWorst.HasValue)
        {
            return allRolls.OrderBy(x => x).Take(keepWorst.Value).ToList();
        }
        else
        {
            return new List<int>(allRolls);
        }
    }

    /// <summary>
    /// Evaluate mathematical expressions with proper PEMDAS order of operations
    /// Supports: +, -, *, /, x (as multiplication)
    /// </summary>
    private int EvaluateMathExpression(string expression)
    {
        try
        {
            // Replace 'x' with '*' for multiplication
            expression = expression.Replace("x", "*");

            // Remove any spaces
            expression = expression.Replace(" ", "");

            // Use DataTable.Compute for PEMDAS evaluation
            var table = new DataTable();
            var result = table.Compute(expression, null);

            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error evaluating math expression: {expression}");
            return 0;
        }
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
            if (result?.Result?.Random?.Data?.Count > 0)
            {
                return result.Result.Random.Data[0];
            }
            else
            {
                _logger.LogWarning("Invalid response from Random.org. Falling back to simulated roll.");
                return SimulatedDiceRoll(min, max);
            }
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
