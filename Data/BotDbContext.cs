using Microsoft.EntityFrameworkCore;

public class BotDbContext : DbContext
{
  public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

  public DbSet<UserRecord> Users { get; set; }
  public DbSet<UserEconomy> Economies { get; set; }
  public DbSet<CurrencyType> CurrencyTypes { get; set; }
  public DbSet<UserCurrencyBalance> CurrencyBalances { get; set; }
  public DbSet<ServerSettings> ServerSettings { get; set; }


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
        .HasOne(s => s.DailyCurrency)
        .WithMany()
        .HasForeignKey(s => s.DailyCurrencyId)
        .OnDelete(DeleteBehavior.Restrict);
  }
}
