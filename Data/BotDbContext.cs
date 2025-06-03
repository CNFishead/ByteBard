using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class BotDbContext : DbContext
{
  public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

  public DbSet<UserRecord> Users { get; set; }
  public DbSet<UserEconomy> Economies { get; set; }
  public DbSet<CurrencyType> CurrencyTypes { get; set; }
  public DbSet<UserCurrencyBalance> CurrencyBalances { get; set; }
  public DbSet<ServerSettings> ServerSettings { get; set; }
  public DbSet<UserGameStats> UserGameStats { get; set; } = null!; public DbSet<CombatTracker> CombatTrackers { get; set; }
  public DbSet<Combatant> Combatants { get; set; }



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
  }
}
