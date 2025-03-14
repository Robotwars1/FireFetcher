using Discord;
using Discord.Interactions;

namespace FireFetcher.Commands;

// Interaction modules must be public and inherit from an IInteractionModuleBase
[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
    // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
    public Program FireFetcher { get; set; }

    [SlashCommand("set-channel", "Sets which channel to send leaderboard in")]
    public async Task SetChannel(ITextChannel Channel)
    {
        await FireFetcher.SetChannel(Channel);

        await RespondAsync($"Leaderboard will be sent in {Channel} from now on", ephemeral: true);
    }

    [SlashCommand("update-leaderboard", "Forces an update of the leaderboard")]
    public async Task UpdateLeaderboard()
    {
        await RespondAsync("Updating leaderboard", ephemeral: true);

        // Respond first since CreateLeaberboard() takes 2-3x the allowed time (6-9s when max allowed is 3s)
        await FireFetcher.CreateLeaderboard();
    }
}
