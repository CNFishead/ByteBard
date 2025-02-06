using Microsoft.EntityFrameworkCore;

public class DatabaseModule : IBaseModule
{
  public void RegisterServices(IServiceCollection services)
  {
    var serviceProvider = services.BuildServiceProvider();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    string connectionString = configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using DB Connection String: {connectionString}"); // Debugging

    services.AddDbContext<BotDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
  }
}
