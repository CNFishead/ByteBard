using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserEconomy")]
public class UserEconomy
{
    [Key] // Since UserId is also the PK, it's marked as Key
    [ForeignKey(nameof(User))] // Reference to the navigation property
    public long UserId { get; set; } // Must match UserRecord.Id (long)

    public virtual required UserRecord User { get; set; } // Correct navigation property

    public int CurrencyAmount { get; set; } = 0;
    public DateTime? LastClaimed { get; set; }
    public int StreakCount { get; set; } = 0;
}
