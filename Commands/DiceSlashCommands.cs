using Discord;
using Discord.Interactions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    [SlashCommand("roll", "Roll dice with advanced features: PEMDAS (3d6x2+1), keep best/worst (4d6b3/4d6w3), advantage/disadvantage")]
    public async Task RollDice(
    [Summary("message", "Dice expressions: 1d20+3 \"attack\" adv; 3d6x2+1; 4d6b3 \"stats\"")] string message)
    {
        try
        {
            await DeferAsync();

            var expressions = message.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var response = new StringBuilder();
            response.AppendLine("🎯 Roll Results!");

            foreach (var rawExpr in expressions)
            {
                var expr = rawExpr.Trim();
                var (diceExpr, label, advMode) = ParseExpression(expr);

                string type = advMode?.ToUpper() ?? "Normal";
                string displayLabel = string.IsNullOrWhiteSpace(label) ? "-" : label;

                // Check if this is an advanced expression (contains PEMDAS operators or keep modifiers)
                bool isAdvancedExpression = ContainsAdvancedFeatures(diceExpr);

                if ((advMode == "adv" || advMode == "disadv") && GetDiceCount(diceExpr) != 1)
                {
                    response.AppendLine($"Type: `ERROR` | Label: `{displayLabel}` | Expression: `{diceExpr}` | ❌ ADV/DISADV requires a single die.");
                    continue;
                }

                if (advMode == "adv" || advMode == "disadv")
                {
                    // Handle advantage/disadvantage
                    var (rolls1, total1, _, mod1) = _diceRoller.Roll(diceExpr);
                    var (rolls2, total2, _, mod2) = _diceRoller.Roll(diceExpr);
                    var chosenTotal = advMode == "adv" ? Math.Max(total1, total2) : Math.Min(total1, total2);

                    var chosenRoll = chosenTotal == total1 ? rolls1[0] : rolls2[0];
                    var otherRoll = chosenTotal == total1 ? rolls2[0] : rolls1[0];

                    string rollsFormatted = chosenTotal == total1
                        ? $"[`{otherRoll}`, `*{chosenRoll}*`]"
                        : $"[`*{chosenRoll}*`, `{otherRoll}`]";

                    response.AppendLine(
                        $"Type: `{type}` | Label: `{displayLabel}` | Expression: `{diceExpr}` | Rolls: {rollsFormatted} +{mod1} → Total: **{chosenTotal}**"
                    );
                }
                else if (isAdvancedExpression)
                {
                    // Handle advanced expressions with PEMDAS and keep best/worst
                    var (allRolls, keptRolls, total, originalExpr) = _diceRoller.RollAdvanced(diceExpr);

                    string rollsDisplay;
                    if (allRolls.Count != keptRolls.Count)
                    {
                        // Show which rolls were kept vs dropped
                        var droppedRolls = allRolls.Except(keptRolls).ToList();
                        var keptRollsFormatted = string.Join("`, `", keptRolls.Select(r => $"*{r}*"));
                        var droppedRollsFormatted = string.Join("`, `", droppedRolls);
                        rollsDisplay = $"All: [`{string.Join("`, `", allRolls)}`] | Kept: [`{keptRollsFormatted}`]";
                    }
                    else
                    {
                        rollsDisplay = $"[`{string.Join("`, `", allRolls)}`]";
                    }

                    response.AppendLine(
                        $"Type: `{type}` | Label: `{displayLabel}` | Expression: `{diceExpr}` | {rollsDisplay} → Total: **{total}**"
                    );
                }
                else
                {
                    // Handle simple expressions
                    var (rolls, total, _, mod) = _diceRoller.Roll(diceExpr);
                    string rollsFormatted = $"[`{string.Join("`, `", rolls)}`]";
                    string modDisplay = mod != 0 ? $" +{mod}" : "";

                    response.AppendLine(
                        $"Type: `{type}` | Label: `{displayLabel}` | Expression: `{diceExpr}` | Rolls: {rollsFormatted}{modDisplay} → Total: **{total}**"
                    );
                }
            }

            await FollowupAsync(response.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RollDice error");
            await FollowupAsync("❌ Something went wrong while preparing your roll.");
            throw;
        }
    }

    /// <summary>
    /// Checks if the dice expression contains advanced features like PEMDAS operators or keep modifiers
    /// </summary>
    private bool ContainsAdvancedFeatures(string diceExpr)
    {
        // Check for multiplication (x or *), keep best (b), keep worst (w), or complex math operations
        return diceExpr.Contains('x') || diceExpr.Contains('*') || diceExpr.Contains('/') ||
               Regex.IsMatch(diceExpr, @"\d+d\d+[bw]\d+") || // keep best/worst patterns
               Regex.IsMatch(diceExpr, @"[+\-]\d+[x*]") || // modifiers with multiplication
               Regex.IsMatch(diceExpr, @"[x*]\d+[+\-]"); // multiplication with modifiers
    }

    /// <summary>
    /// Parses expression into diceExpr, optional label (in quotes), and optional mode (adv/disadv).
    /// </summary>
    private (string diceExpr, string label, string? advantageMode) ParseExpression(string expr)
    {
        string dicePart = expr;
        string labelPart = "";
        string? advantage = null;

        int firstQuoteIndex = expr.IndexOf('"');
        int secondQuoteIndex = expr.IndexOf('"', firstQuoteIndex + 1);
        if (firstQuoteIndex >= 0 && secondQuoteIndex > firstQuoteIndex)
        {
            labelPart = expr.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1).Trim();
            dicePart = expr.Substring(0, firstQuoteIndex).Trim() + " " + expr.Substring(secondQuoteIndex + 1).Trim();
        }

        var parts = dicePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var lastPart = parts[^1].ToLower();
            if (lastPart == "adv" || lastPart == "disadv")
            {
                advantage = lastPart;
                dicePart = string.Join(" ", parts[..^1]);
            }
        }

        return (dicePart.Trim(), labelPart.Trim(), advantage);
    }

    /// <summary>
    /// Extracts the number of dice rolled from expressions like "2d20+3"
    /// </summary>
    private int GetDiceCount(string diceExpr)
    {
        var parts = diceExpr.Split('d');
        if (parts.Length < 2) return 0;
        return int.TryParse(parts[0], out int count) ? count : 0;
    }
}
