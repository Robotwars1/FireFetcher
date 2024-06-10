using Discord;
using Discord.Interactions;

namespace FireFetcher.Commands;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public class GeneralCommands : InteractionModuleBase<SocketInteractionContext>
{
    // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
    public InteractionService Commands { get; set; }

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
        Embed.AddField("/ping", "a");
        Embed.AddField("/help", "a");
        Embed.AddField("/link-accounts", "a");
        Embed.AddField("/remove-self", "a");
        Embed.AddField("/set-nickname", "a");
        Embed.AddField("/list-users", "a");
        Embed.AddField("Admin Commands\n/set-channel", "a");
        Embed.AddField("/update-leaderboard", "a");

        await RespondAsync(embed: Embed.Build());
    }
}
