using Discord;
using Discord.Interactions;
using FallVerseBotV2.Commands.Admin;

[Group("skill", "Commands related to managing skills and abilities.")]
public class SkillModule : BaseGroupModule
{
  private readonly ILoggerFactory _loggerFactory;
  private readonly SkillCheckHandler _skillHandler;
  private readonly SkillCheckLifecycleHandler _lifecycleHandler;
  private readonly SkillHelpHandler _helpHandler;

  public SkillModule(ILogger<BaseGroupModule> logger, ILoggerFactory loggerFactory, BotDbContext db)
      : base(logger, db)
  {
    _loggerFactory = loggerFactory;
    _skillHandler = new SkillCheckHandler(loggerFactory.CreateLogger<SkillCheckHandler>(), db);
    _lifecycleHandler = new SkillCheckLifecycleHandler(loggerFactory.CreateLogger<SkillCheckLifecycleHandler>(), db);
    _helpHandler = new SkillHelpHandler(loggerFactory.CreateLogger<SkillHelpHandler>());
  }

  [SlashCommand("init-skillcheck", "Create a new skill check for players to attempt.")]
  public async Task InitSkillCheck(
      string skill,
      int dc,
      string? successMessage = null,
      string? failureMessage = null,
      int? durationMinutes = null,
      bool isPrivate = false)
    => await _lifecycleHandler.InitSkillCheck(Context, skill, dc, successMessage, failureMessage, durationMinutes, isPrivate);

  [SlashCommand("remove-skillcheck", "Remove an active skill check.")]
  public async Task RemoveSkillCheck(string skill)
    => await _lifecycleHandler.RemoveSkillCheck(Context, skill);

  [SlashCommand("list-skillchecks", "List all active skill checks in this channel.")]
  public async Task ListSkillChecks()
    => await _lifecycleHandler.ListSkillChecks(Context);

  [SlashCommand("roll", "Attempt a skill check with your roll result.")]
  public async Task RollSkillCheck(string skill, int rollResult)
    => await _skillHandler.HandleSkillRoll(Context, skill, rollResult);

  [SlashCommand("status", "Show the status of a specific skill check.")]
  public async Task ShowSkillCheckStatus(string skill)
    => await _skillHandler.ShowSkillCheckStatus(Context, skill);

  [SlashCommand("help", "Get help and documentation for skill check commands.")]
  public async Task ShowHelp()
    => await _helpHandler.ShowHelp(Context);

  [SlashCommand("help-privacy", "Learn about privacy modes and information security.")]
  public async Task ShowPrivacyHelp()
    => await _helpHandler.ShowPrivacyHelp(Context);

  [SlashCommand("help-examples", "See examples of skill check usage.")]
  public async Task ShowExamples()
    => await _helpHandler.ShowExamples(Context);
}