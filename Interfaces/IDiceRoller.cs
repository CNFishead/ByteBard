public interface IDiceRoller
{
    (List<int> rolls, int total, int faces, int modifier) Roll(string diceExpr);
    (List<int> allRolls, List<int> keptRolls, int total, string expression) RollAdvanced(string diceExpr);
}