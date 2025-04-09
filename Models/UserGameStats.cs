using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserGameStats
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    public string GameKey { get; set; } = string.Empty; // e.g., "diceduel", "coinflip"

    [Required]
    public ulong GuildId { get; set; }

    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int Ties { get; set; } = 0;

    public int TotalGames => Wins + Losses + Ties;

    public int TotalWagered { get; set; } = 0;
    public int NetGain { get; set; } = 0; // Can be negative

    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual UserRecord User { get; set; } = null!;
}
