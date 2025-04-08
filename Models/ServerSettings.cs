using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ServerSettings
{
  [Key]
  public ulong GuildId { get; set; } // For per-server settings

  [ForeignKey(nameof(DailyCurrency))]
  public int DailyCurrencyId { get; set; }
  [ForeignKey(nameof(CasinoCurrency))]
  public int? CasinoCurrencyId { get; set; }

  public virtual CurrencyType? CasinoCurrency { get; set; }
  public virtual CurrencyType DailyCurrency { get; set; } = null!;
}
