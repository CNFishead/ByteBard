using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        db.Database.Migrate(); // Applies any pending migrations
    }
}
