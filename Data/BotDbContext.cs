using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BotDbContext : DbContext
{
  public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

  public DbSet<UserRecord> Users { get; set; }
  public DbSet<UserEconomy> Economies { get; set; }
  public DbSet<CurrencyType> CurrencyTypes { get; set; }
  public DbSet<UserCurrencyBalance> CurrencyBalances { get; set; }
  public DbSet<ServerSettings> ServerSettings { get; set; }
  public DbSet<UserGameStats> UserGameStats { get; set; } = null!;
  public DbSet<CombatTracker> CombatTrackers { get; set; }
  public DbSet<Combatant> Combatants { get; set; }
  public DbSet<SkillCheck> SkillChecks { get; set; }
  public DbSet<SkillCheckAttempt> SkillCheckAttempts { get; set; }



  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<UserEconomy>()
    .HasKey(e => e.UserId);

    modelBuilder.Entity<UserEconomy>()
        .HasOne(e => e.User)
        .WithOne()
        .HasForeignKey<UserEconomy>(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserCurrencyBalance>()
        .HasIndex(e => new { e.UserId, e.CurrencyTypeId })
        .IsUnique();

    modelBuilder.Entity<ServerSettings>()
    .HasKey(s => s.GuildId);

    modelBuilder.Entity<ServerSettings>()
        .Property(s => s.DefaultJoinRoleIds)
        .HasColumnType("jsonb");
    modelBuilder.Entity<ServerSettings>()
        .HasOne(s => s.DailyCurrency)
        .WithMany()
        .HasForeignKey(s => s.DailyCurrencyId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<UserGameStats>()
        .Property(e => e.LastGameData)
        .HasColumnType("jsonb")
        .HasDefaultValueSql("'{}'::jsonb");

    modelBuilder.Entity<CombatTracker>()
        .HasIndex(t => new
        {
          t.GuildId,
          t.ChannelId,
          t.GameId
        })
        .IsUnique();

    modelBuilder.Entity<CombatTracker>()
        .HasIndex(t => t.LastUpdatedAt);

    // Force all DateTime values to UTC
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
      foreach (var property in entity.GetProperties())
      {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
          property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
              v => v.ToUniversalTime(),
              v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
      }
    }

    // Skill Check configurations
    modelBuilder.Entity<SkillCheck>()
        .HasMany(sc => sc.Attempts)
        .WithOne(sca => sca.SkillCheck)
        .HasForeignKey(sca => sca.SkillCheckId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<SkillCheckAttempt>()
        .HasIndex(sca => new { sca.SkillCheckId, sca.UserId })
        .IsUnique(); // Prevent duplicate attempts by same user
  }

  private void UpdateTimestamps()
  {
    var entries = ChangeTracker.Entries<CombatTracker>()
      .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
      entry.Entity.LastUpdatedAt = DateTime.UtcNow;
    }
  }

  public override int SaveChanges()
  {
    UpdateTimestamps();
    return base.SaveChanges();
  }

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    UpdateTimestamps();
    return base.SaveChangesAsync(cancellationToken);
  }
}
