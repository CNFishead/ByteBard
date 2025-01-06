using Microsoft.AspNetCore.Mvc;

namespace MyBotBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class BotController : ControllerBase
  {
    private readonly DiscordBotService _botService;

    public BotController(DiscordBotService botService)
    {
      _botService = botService;
    }

    // Example: GET /api/bot/status
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
      // log to the console that a requewst was made
      Console.WriteLine("Request made to /api/bot/status");
      // Return some basic info about the bot’s connection or presence
      // If the service exposed a public method or property for status, you can read it here
      return Ok(new { Status = "Bot is running" });
    }

    // Example: POST /api/bot/send
    [HttpPost("send")]
    public IActionResult SendMessage([FromBody] BotMessageRequest request)
    {
      // You would implement a method on your bot service to send a message
      // For example:
      // _botService.SendChannelMessage(request.ChannelId, request.Content);

      return Ok("Message sent!");
    }

    [HttpGet("roll-dice")]
    public IActionResult RollDice()
    {
      // Build array of dice rolls
      var rollList = new List<int>();

      // Roll the dice 100 times
      for (int i = 0; i < 100; i++)
      {
        var random = new Random();
        // Generate a random number for the amount of times the dice will bounce
        var times = random.Next(1, 25); // 1 to 25 times
        int roll = 0;

        // Simulate multiple bounces
        for (int y = 0; y < times; y++)
        {
          roll += random.Next(1, 20 + 1); // Roll a 20-sided die
        }

        // Normalize the roll to a single valid 20-sided die result
        roll = (roll % 20) + 1;

        // Add to the list
        rollList.Add(roll);
      }

      // Return the results as a JSON array
      return Ok(rollList);
    }
  }

  // Simple DTO for the request body
  public class BotMessageRequest
  {
    public ulong ChannelId { get; set; }
    public string Content { get; set; }
  }
}
