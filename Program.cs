var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
IBaseModule[] modules = new IBaseModule[]
{
    new BotModule(),
};

// dynamically register services from modules
foreach (var module in modules)
{
    module.RegisterServices(builder.Services);
}


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<IDiceRoller, RandomOrgDiceRoller>();

var app = builder.Build();
app.Logger.LogInformation("Built");

// Access the bot service and start it
var botService = app.Services.GetRequiredService<DiscordBotService>();
await botService.StartAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// without this, the app will not be able to handle requests
app.MapControllers();

app.UseHttpsRedirection();
app.Run();