using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserRecord")]
public class UserRecord
{
    [Key]
    // auto-incrementing primary key
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public ulong DiscordId { get; set; }
    [Required]
    public string Username { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow; // sets the joined at time to the current time
}