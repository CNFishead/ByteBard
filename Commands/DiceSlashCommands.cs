using Discord.Interactions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

public class DiceRollModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger;
    private readonly IDiceRoller _diceRoller;

    public DiceRollModule(ILogger<DiceRollModule> logger, IDiceRoller diceRoller)
    {
        _logger = logger;
        _diceRoller = diceRoller;
        _logger.LogInformation("DiceRollModule constructed");
    }

    [SlashCommand("roll", "Roll dice from a user-supplied expression (e.g., 2d20+2 \"attack\")")]
    public async Task RollDice(
        [Summary("message", "Dice expressions, e.g. 2d20+2 \"attack\"; 4d4+2 \"damage\"")] string message)
    {
        try
        {
            // incase the command takes too long to respond, defer
            await DeferAsync();

            _logger.LogInformation("RollDice fired.");
            // Split multiple expressions by semicolon
            var expressions = message.Split(';', StringSplitOptions.RemoveEmptyEntries);

            // We'll build a single response for all expressions
            // Each expression is one line, using commas and backticks.
            var responseBuilder = new StringBuilder();

            foreach (var rawExpr in expressions)
            {
                // _logger.LogInformation($"Processing expression: {rawExpr}");
                var expr = rawExpr.Trim();
                var (diceExpr, label) = ParseExpression(expr);
                // _logger.LogInformation($"Parsed: diceExpr={diceExpr}, label={label}");
                // Roll the expression, capturing the full breakdown
                var (rolls, total, faces, modifier) = _diceRoller.Roll(diceExpr);
                _logger.LogInformation($"Rolls: {string.Join(", ", rolls)}, Total: {total}, Faces: {faces}, Modifier: {modifier}");

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

            await FollowupAsync(responseBuilder.ToString());

        }

        catch (System.Exception ex)
        {
            _logger.LogError(ex, "DeferAsync failed.");
            await RespondAsync("❌ Something went wrong while preparing your command.");
            throw; // Rethrow the exception to be handled by the global exception handler
        }
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

}
