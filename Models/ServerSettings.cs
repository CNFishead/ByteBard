using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ServerSettings
{
  [Key]
  public ulong GuildId { get; set; } // For per-server settings

  [ForeignKey(nameof(DailyCurrency))]
  public int DailyCurrencyId { get; set; }

  public virtual CurrencyType DailyCurrency { get; set; } = null!;
}
