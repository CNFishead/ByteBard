using Discord.Commands;
using Discord.Interactions;
using FallVerseBotV2.Commands.Economy;
using FallVerseBotV2.Commands.Economy.Casino.Handlers;
using FallVerseBotV2.Enums;

[Discord.Interactions.Group("casino", "Casino-related commands.")]
public class CasinoModule : BaseCasinoModule
{
  public CasinoModule(ILogger<BaseCasinoModule> logger, BotDbContext db)
      : base(logger, db)
  {
    _diceDuel = new DiceDuelHandler(logger, db);
    _coinFlip = new CoinFlipHandler(logger, db);
    _setCasinoCurrencyHandler = new SetCasinoCurrencyHandler(logger, db);
  } 

  private readonly DiceDuelHandler _diceDuel;
  private readonly CoinFlipHandler _coinFlip;
  private readonly SetCasinoCurrencyHandler _setCasinoCurrencyHandler;

  [SlashCommand("diceduel", "Play a dice duel.")]
  public async Task DiceDuel(int amount) => await _diceDuel.Run(Context, amount);

  [SlashCommand("flip", "Flip a coin.")]
  public async Task CoinFlip(CoinSide side, int amount) => await _coinFlip.Run(Context, side, amount);

  [SlashCommand("setcurrency", "Sets the currency to be used for casino games.")]
  public async Task SetCasinoCurrency(string currency) => await _setCasinoCurrencyHandler.Run(Context, currency);
}
