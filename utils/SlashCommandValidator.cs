using Discord.Interactions;

public class SlashCommandValidator
{
  public void ValidateCommands(InteractionService interactionService)
  {
    foreach (var command in interactionService.SlashCommands)
    {
      ValidateCommand(command);
    }
  }

  private void ValidateCommand(SlashCommandInfo command)
  {
    if (command.Name.Length > 32)
      Console.WriteLine($"❌ Command `{command.Name}` name exceeds 32 characters.");

    if (command.Description.Length > 100)
      Console.WriteLine($"❌ Command `{command.Name}` description exceeds 100 characters.");

    if (command.Parameters.Count > 25)
      Console.WriteLine($"❌ Command `{command.Name}` has more than 25 options.");

    foreach (var param in command.Parameters)
    {
      if (param.Name.Length > 32)
        Console.WriteLine($"❌ Param `{param.Name}` in `{command.Name}` exceeds 32 characters.");

      if (param.Description?.Length > 100)
        Console.WriteLine($"❌ Param `{param.Name}` description in `{command.Name}` exceeds 100 characters.");
    }
  }
}
