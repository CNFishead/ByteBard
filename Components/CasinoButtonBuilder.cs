using Discord;

public static class CasinoButtonBuilder
{
    public static MessageComponent BuildPlayAgainButton(string gameKey, string? label = null)
    {
        return new ComponentBuilder()
            .WithButton(
                label ?? "Play Again",
                customId: $"casino_{gameKey}_playagain",
                style: ButtonStyle.Primary
            ).Build();
    }
}
