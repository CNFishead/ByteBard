using Microsoft.EntityFrameworkCore;

public class BotDbContext : DbContext
{
  public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

  public DbSet<UserRecord> Users { get; set; }
  public DbSet<UserEconomy> Economies { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<UserEconomy>()
        .HasOne<UserRecord>()
        .WithOne()
        .HasForeignKey<UserEconomy>(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
