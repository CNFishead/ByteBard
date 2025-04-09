using Discord.Commands;
using Discord.Interactions;
using FallVerseBotV2.Commands.Economy;
using FallVerseBotV2.Commands.Economy.Casino.Handlers;
using FallVerseBotV2.Enums;
using Microsoft.Extensions.Logging;

[Discord.Interactions.Group("casino", "Casino-related commands.")]
public class CasinoModule : BaseCasinoModule
{
  private readonly ILoggerFactory _loggerFactory;

  public CasinoModule(ILogger<BaseCasinoModule> logger, ILoggerFactory loggerFactory, BotDbContext db)
      : base(logger, db)
  {
    _loggerFactory = loggerFactory;
    _diceDuel = new DiceDuelHandler(_loggerFactory.CreateLogger<DiceDuelHandler>(), db);
    _coinFlip = new CoinFlipHandler(_loggerFactory.CreateLogger<CoinFlipHandler>(), db);
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
