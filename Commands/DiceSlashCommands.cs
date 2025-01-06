using Discord.Interactions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

public class DiceRollModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string RandomOrgBaseUrl = "https://api.random.org/json-rpc/4/invoke";


    [SlashCommand("roll", "Roll dice from a user-supplied expression (e.g., 2d20+2 \"attack\")")]
    public async Task RollDice(
        [Summary("message", "Dice expressions, e.g. 2d20+2 \"attack\"; 4d4+2 \"damage\"")]
        string message)
    {
        Console.WriteLine("RollDice fired.");
        // Split multiple expressions by semicolon
        var expressions = message.Split(';', StringSplitOptions.RemoveEmptyEntries);

        // We'll build a single response for all expressions
        // Each expression is one line, using commas and backticks.
        var responseBuilder = new StringBuilder();

        foreach (var rawExpr in expressions)
        {
            Console.WriteLine($"Processing expression: {rawExpr}");
            var expr = rawExpr.Trim();
            var (diceExpr, label) = ParseExpression(expr);

            // Roll the expression, capturing the full breakdown
            var (rolls, total, faces, modifier) = RollDiceExpression(diceExpr);

            Console.WriteLine($"Rolls: {string.Join(", ", rolls)}");

            // Format the list of individual rolls, e.g. [9, 2]
            var rollsString = $"[{string.Join(", ", rolls)}]";

            // Build one line with backticks
            // Example:
            // @SomeUser `Request: 2d20+2, Rolls: [9, 2], Result: 13, Label: attack`
            var lineBuilder = new StringBuilder();
            lineBuilder.Append(Context.User.Mention).Append(" ");
            // lineBuilder.Append("`"); 

            if (!string.IsNullOrWhiteSpace(label))
            {
                lineBuilder.Append($"Label: `{label}`");
            }
            lineBuilder.Append($", Request: `{diceExpr}`, Rolls: `{rollsString}`, Result: `{total}`");

            // lineBuilder.Append("`"); // end backtick

            responseBuilder.AppendLine(lineBuilder.ToString());
        }

        await RespondAsync(responseBuilder.ToString());
    }

    /// <summary>
    /// Extract the dice expression ("2d20+2") and optional label ("attack") 
    /// from something like: 2d20+2 "attack"
    /// </summary>
    private (string diceExpr, string label) ParseExpression(string expr)
    {
        // Simple approach:
        // 1) Find a quoted substring => label
        // 2) Everything before that is the dice expression
        string dicePart = expr;
        string labelPart = "";

        int firstQuoteIndex = expr.IndexOf('"');
        if (firstQuoteIndex >= 0)
        {
            int secondQuoteIndex = expr.IndexOf('"', firstQuoteIndex + 1);
            if (secondQuoteIndex > firstQuoteIndex)
            {
                labelPart = expr.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                dicePart = expr.Substring(0, firstQuoteIndex).Trim();
            }
        }

        return (dicePart.Trim(), labelPart.Trim());
    }

    /// <summary>
    /// Parse a dice expression like "2d20+2" and roll each die individually.
    /// Return the list of rolls, total, faces, and modifier.
    /// </summary>
    private (List<int> rolls, int total, int faces, int modifier) RollDiceExpression(string diceExpr)
    {
        // Format: [X]d[Y](+Z)
        // e.g. "2d20+2" => 2 dice, 20 faces, +2
        // We'll parse it in a naive way: split on 'd', then check for '+'
        Console.WriteLine($"RollDiceExpression: {diceExpr}");
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
            Console.WriteLine($"Rolling die {i + 1}");
            int rollValue = RollDiceUsingApi(1, 1, faces).Result;
            Console.WriteLine($"Roll: {rollValue}");
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

            // Parse the response JSON
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RandomOrgResponse>(responseJson);

            // Return the first number in the result
            return result.result.random.data[0];
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to fetch from Random.org. Falling back to simulated roll.");
            // console log the error message from the api
            Console.WriteLine("Error message: " + e.Message);
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
    
    // DTO to deserialize the Random.org response
    private class RandomOrgResponse
    {
        public RandomOrgResult result { get; set; }

        public class RandomOrgResult
        {
            public RandomData random { get; set; }

            public class RandomData
            {
                public int[] data { get; set; }
            }
        }
    }
}
