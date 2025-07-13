using System.ComponentModel.DataAnnotations;

public class SkillCheck
{
  public int Id { get; set; }
  public ulong GuildId { get; set; }
  public ulong ChannelId { get; set; }
  public string SkillName { get; set; } = string.Empty;
  public int DC { get; set; } // Difficulty Class
  public string? SuccessMessage { get; set; }
  public string? FailureMessage { get; set; }
  public ulong CreatedByUserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public DateTime? ExpiresAt { get; set; } // Optional expiration
  public bool IsActive { get; set; } = true;
  public bool IsPrivate { get; set; } = false; // If true, results are only sent via DM

  // Track who has already attempted this skill check
  public List<SkillCheckAttempt> Attempts { get; set; } = new();
}

public class SkillCheckAttempt
{
  public int Id { get; set; }
  public int SkillCheckId { get; set; }
  public SkillCheck SkillCheck { get; set; } = null!;
  public ulong UserId { get; set; }
  public int RollResult { get; set; }
  public bool Success { get; set; }
  public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
