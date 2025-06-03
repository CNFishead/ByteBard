public static class TurnProcessor
{
  public record TurnResult(string Message, ulong? MentionUserId);

  public static TurnResult? Advance(CombatTracker tracker)
  {
    if (!tracker.IsActive)
      return null;

    if (tracker.TurnQueue.Count == 0)
    {
      tracker.CurrentRound++;
      tracker.TurnQueue = tracker.Combatants
          .OrderByDescending(c => c.Initiative)
          .Select(c => c.Id)
          .ToList();
      tracker.CurrentTurnIndex = 0;
    }

    if (!tracker.TurnQueue.Any())
      return null;

    var nextId = tracker.TurnQueue.First();
    tracker.TurnQueue.RemoveAt(0);
    tracker.CurrentTurnIndex++;

    var combatant = tracker.Combatants.FirstOrDefault(c => c.Id == nextId);
    if (combatant == null)
      return new TurnResult("⚠️ Next combatant not found. Turn skipped.", null);

    var mention = $"<@{combatant.DiscordUserId}>";
    var name = string.IsNullOrWhiteSpace(combatant.Name) ? "Unknown Combatant" : combatant.Name;

    var message = $"🔁 Round {tracker.CurrentRound}, Turn {tracker.CurrentTurnIndex}\n{mention}, your character `{name}` is up.";
    return new TurnResult(message, combatant.DiscordUserId);
  }

}
