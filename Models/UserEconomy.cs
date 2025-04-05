using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserEconomy")]
public class UserEconomy
{
    [Key]
    public long UserId { get; set; }

    public virtual UserRecord User { get; set; } = null!;

    public int CurrencyAmount { get; set; } = 0;
    public DateTime? LastClaimed { get; set; }
    public int StreakCount { get; set; } = 0;
}
