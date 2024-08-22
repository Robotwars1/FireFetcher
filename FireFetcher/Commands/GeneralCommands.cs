using Discord;
using Discord.Interactions;

namespace FireFetcher.Commands;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public class GeneralCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Get bot latency")]
    public async Task Ping()
    {
        var Embed = new EmbedBuilder();
        Embed.AddField("Yes I'm alive, now bugger off", $"Latency: {Context.Client.Latency}ms");

        await RespondAsync(embed: Embed.Build());
    }

    [SlashCommand("help", "Show commands and what they do")]
    public async Task Help()
    {
        var Embed = new EmbedBuilder();
        Embed.AddField("/ping", "Gets bot latency");
        Embed.AddField("/help", "Shows this list");
        Embed.AddField("/link-accounts", "Links accounts for the leaderboards");
        Embed.AddField("/set-nickname", "Set nickname for leaderboards");
        Embed.AddField("/remove-self", "Removes self from the leaderboard");
        Embed.AddField("/list-users", "Lists each added user");
        Embed.AddField("Admin Commands\n/set-channel", "Sets which channel to send leaderboard in");
        Embed.AddField("/update-leaderboard", "Forces an update of the leaderboard");

        await RespondAsync(embed: Embed.Build());
    }
}
