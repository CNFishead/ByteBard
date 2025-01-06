using Discord.Interactions;
using System.Text;
using System.Threading.Tasks;

public class DiceRollModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("roll", "Roll dice from a user-supplied expression (e.g., 2d20+2 \"attack\")")]
    public async Task RollDice(
        [Summary("message", "Dice expressions, e.g. 2d20+2 \"attack\"; 4d4+2 \"damage\"")] 
        string message)
    {
        // Split multiple expressions by semicolon
        var expressions = message.Split(';', StringSplitOptions.RemoveEmptyEntries);

        // We'll build a single response for all expressions
        // Each expression is one line, using commas and backticks.
        var responseBuilder = new StringBuilder();

        foreach (var rawExpr in expressions)
        {
            var expr = rawExpr.Trim();
            var (diceExpr, label) = ParseExpression(expr);

            // Roll the expression, capturing the full breakdown
            var (rolls, total, faces, modifier) = RollDiceExpression(diceExpr);

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
            int rollValue = random.Next(1, faces + 1);
            rollList.Add(rollValue);
        }

        // Sum up the dice, then add the modifier
        int total = rollList.Sum() + modifier;

        return (rollList, total, faces, modifier);
    }
}
