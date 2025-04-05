using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserCurrencyBalance")]
public class UserCurrencyBalance
{
  [Key]
  public int Id { get; set; }

  [ForeignKey(nameof(User))]
  public long UserId { get; set; }

  [ForeignKey(nameof(CurrencyType))]
  public int CurrencyTypeId { get; set; }

  public int Amount { get; set; } = 0;

  public virtual UserRecord User { get; set; } = null!;
  public virtual CurrencyType CurrencyType { get; set; } = null!;
}
