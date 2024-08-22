using Discord;
using Discord.Interactions;

namespace FireFetcher.Commands;

// Interaction modules must be public and inherit from an IInteractionModuleBase
[CommandContextType(InteractionContextType.Guild)]
public class UserCommands : InteractionModuleBase<SocketInteractionContext>
{
    // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
    public UserHandler UserHandler { get; set; }

    [SlashCommand("link-accounts", "Links accounts for the leaderboards")]
    public async Task LinkAccount(string SrcUsername, string BoardProfileID, string SteamUsername)
    {
        ulong UserID = Context.User.Id;
        string Username = Context.User.Username;

        UserHandler.LinkAccount(UserID, Username, SrcUsername, BoardProfileID, SteamUsername);

        await RespondAsync($"Updated linked accounts for {Context.User}", ephemeral: true);
    }

    [SlashCommand("set-nickname", "Set nickname for leaderboards")]
    public async Task SetNickname(string Nickname)
    {
        ulong UserID = Context.User.Id;

        // Wait for task to return var
        var task = UserHandler.SetNickname(UserID, Nickname);
        await task;
        bool HasLinkedAcc = task.Result;

        // If user hasnt linked any accounts
        if (!HasLinkedAcc)
        {
            await RespondAsync($"User has no accounts linked, cannot set Nickname", ephemeral: true);
        }
        else
        {
            await RespondAsync($"Changed nickname for {Context.User}", ephemeral: true);
        }
    }

    [SlashCommand("remove-self", "Removes self from the leaderboard")]
    public async Task RemoveSelf()
    {
        ulong UserID = Context.User.Id;

        // Wait for task to return var
        var task = UserHandler.UnlinkAccount(UserID);
        await task;
        bool HasLinkedAcc = task.Result;

        // If user hasnt linked any accounts
        if (!HasLinkedAcc)
        {
            await RespondAsync($"User has no accounts linked", ephemeral: true);
        }
        else
        {
            await RespondAsync($"Unlinked accounts for {Context.User}", ephemeral: true);
        }
    }

    [SlashCommand("list-users", "Lists each added user")]
    public async Task ListUsers()
    {
        string[] Users = UserHandler.GetDiscordUsernames();

        string Text = "";
        for (int i = 0; i < Users.Length; i++)
        {
            // If not first line, make sure everything gets a newline
            if (i > 0)
            {
                Text += '\n';
            }

            Text += Users[i];
        }

        if (Users.Length == 0)
        {
            Text = "No users are added";
        }

        var Embed = new EmbedBuilder();
        Embed.AddField("Users on leaderbaords", Text);

        await RespondAsync(embed: Embed.Build());
    }
}
