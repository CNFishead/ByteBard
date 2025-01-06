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
  }

  // Simple DTO for the request body
  public class BotMessageRequest
  {
    public ulong ChannelId { get; set; }
    public string Content { get; set; }
  }
}
