using Discord;
using Discord.Interactions;
using FallVerseBotV2.Commands.Admin;

[Group("combat", "Commands related to managing combat and initiative tracking.")]
public class CombatModule : BaseGroupModule
{
  private readonly ILoggerFactory _loggerFactory;
  private readonly TrackerLifecycleHandler _trackerHandler;
  private readonly CombatantHandler _combatantHandler;
  private readonly TurnOrderHandler _turnHandler;

  public CombatModule(ILogger<BaseGroupModule> logger, ILoggerFactory loggerFactory, BotDbContext db)
      : base(logger, db)
  {
    _loggerFactory = loggerFactory;

    _trackerHandler = new TrackerLifecycleHandler(loggerFactory.CreateLogger<TrackerLifecycleHandler>(), db);
    _combatantHandler = new CombatantHandler(loggerFactory.CreateLogger<CombatantHandler>(), db);
    _turnHandler = new TurnOrderHandler(loggerFactory.CreateLogger<TurnOrderHandler>(), db);
  }

  [SlashCommand("tracker-init", "Create a new combat tracker in this channel.")]
  public async Task InitTracker(string gameId)
    => await _trackerHandler.CreateTracker(Context, gameId);

  [SlashCommand("tracker-start", "Start the combat tracker.")]
  public async Task StartTracker(string gameId)
    => await _trackerHandler.StartTracker(Context, gameId);

  [SlashCommand("tracker-reset", "Reset the tracker for the current game.")]
  public async Task ResetTracker(string gameId)
    => await _trackerHandler.Reset(Context, gameId);

  [SlashCommand("tracker-delete", "Delete the tracker and its combatants.")]
  public async Task DeleteTracker(string gameId)
    => await _trackerHandler.Delete(Context, gameId);


  [SlashCommand("tracker-add", "Add a combatant to the tracker.")]
  public async Task AddCombatant(string gameId, int initiative, string? name = null)
    => await _combatantHandler.AddCombatant(Context, gameId, initiative, name);
  [SlashCommand("tracker-remove", "Remove a combatant from the tracker by name or ID.")]
  public async Task RemoveCombatant(string gameId, string combatantNameOrId)
    => await _combatantHandler.RemoveCombatant(Context, gameId, combatantNameOrId);

  [SlashCommand("tracker-next", "Advance to the next turn.")]
  public async Task NextTurn(string gameId)
    => await _turnHandler.Advance(Context, gameId);

  [SlashCommand("tracker-setturn", "Set the current turn to a specific index.")]
  public async Task SetTurn(string gameId, int turnIndex)
    => await _turnHandler.SetTurn(Context, gameId, turnIndex);
}
