using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CurrencyType")]
public class CurrencyType
{
  [Key]
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty; 
  [Required]
  public ulong GuildId { get; set; }
}
