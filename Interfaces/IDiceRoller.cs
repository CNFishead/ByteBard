public interface IDiceRoller
{
    (List<int> rolls, int total, int faces, int modifier) Roll(string diceExpr);
}