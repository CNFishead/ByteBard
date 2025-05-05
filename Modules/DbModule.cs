using Microsoft.EntityFrameworkCore;
using Npgsql;

public class DatabaseModule : IBaseModule
{
  public void RegisterServices(IServiceCollection services)
  {
    var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using DB Connection String: {connectionString}"); // Debugging

    services.AddDbContext<BotDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
    // ✅ Enable dynamic JSON reading
    NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
  }
}
