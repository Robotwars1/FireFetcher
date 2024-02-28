using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text.Json;

// Thingy to call other classes (in other .cs files)
using FireFetcher;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private readonly JsonInterface JsonInterface = new();
    private readonly ApiRequester ApiRequester = new();

    private DiscordSocketClient Client;

    IMessageChannel Channel;
    ulong? LeaderboardMessageId;
    Dictionary<string, string> Users = new();

    // Paths to each .json file
    const string UsersFilePath = "Data/Users.json";
    const string MessageFilePath = "Data/Message.json";
    const string ChannelFilePath = "Data/Channel.json";

    private readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Classes for cleaned data
    public class CleanedResponse
    {
        public string Runner { get; set; }
        public string Partner { get; set; }
        public int Place { get; set; }
        public string Time { get; set; }
        public string Map { get; set; }
        public int PortalCount { get; set; }
    }

    public async Task MainAsync()
    {
        Client = new DiscordSocketClient();

        // Setup logging
        Client.Log += Log;

        // Read bot-token
        // DO NOT MAKE TOKEN PUBLIC
        var token = File.ReadAllText("Token.txt");

        // Read saved data
        Users = (Dictionary<string, string>)JsonInterface.ReadJson(UsersFilePath, "Users");
        LeaderboardMessageId = (ulong?)JsonInterface.ReadJson(MessageFilePath, "ID");

        // Start bot
        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        // Hooking up more commands
        Client.Ready += Client_Ready;
        Client.SlashCommandExecuted += SlashCommandHandler;

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Logger Logger = new();
        Logger.GeneralLog(msg.ToString());

        return Task.CompletedTask;
    }

    public async Task Client_Ready()
    {
        var PingCommand = new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("Get bot latency");

        // Command for setting which server channel to send leaderboard in
        var SetChannelCommand = new SlashCommandBuilder()
            .WithName("set-channel")
            .WithDescription("Sets which channel to send leaderboard in")
            .AddOption("channel", ApplicationCommandOptionType.Channel, "Channel", isRequired: true);

        // Command for adding user to leaderboard
        var AddUserCommand = new SlashCommandBuilder()
            .WithName("add-user")
            .WithDescription("Adds a user to be included in the leaderboard updates")
            .AddOption("src-username", ApplicationCommandOptionType.String, "Speedrun.com username", isRequired: true)
            .AddOption("cm-board-username", ApplicationCommandOptionType.String, "board.portal2.sr username", isRequired: true);

        // Command for removing user from leaderboard
        var RemoveUserCommand = new SlashCommandBuilder()
            .WithName("remove-user")
            .WithDescription("Removes a user from the leaderboard")
            .AddOption("src-username", ApplicationCommandOptionType.String, "Speedrun.com username", isRequired: true)
            .AddOption("cm-board-username", ApplicationCommandOptionType.String, "board.portal2.sr username", isRequired: true);

        var ListUsersCommand = new SlashCommandBuilder()
            .WithName("list-users")
            .WithDescription("Lists each added user");

        // Command for updating leaderboard
        var UpdateLeaderboardCommand = new SlashCommandBuilder()
            .WithName("update-leaderboard")
            .WithDescription("Forces an update of the leaderboard");

        try
        {
            // Create each slash command
            await Client.CreateGlobalApplicationCommandAsync(PingCommand.Build());
            await Client.CreateGlobalApplicationCommandAsync(SetChannelCommand.Build());
            await Client.CreateGlobalApplicationCommandAsync(AddUserCommand.Build());
            await Client.CreateGlobalApplicationCommandAsync(RemoveUserCommand.Build());
            await Client.CreateGlobalApplicationCommandAsync(ListUsersCommand.Build());
            await Client.CreateGlobalApplicationCommandAsync(UpdateLeaderboardCommand.Build());
        }
        catch (HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }

        // Gets channel id later than everything else cause it doesnt work otherwise ¯\_(ツ)_/¯
        ulong? ChannelId = (ulong?)JsonInterface.ReadJson(ChannelFilePath, "ID");
        if (ChannelId != null)
        {
            Channel = Client.GetChannel((ulong)ChannelId) as IMessageChannel;
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand Command)
    {
        Logger Logger = new();
        Logger.CommandLog(Command.Data.Name, Command.User.ToString());

        switch (Command.Data.Name)
        {
            case "ping":
                await HandlePingCommand(Command);
                break;
            case "set-channel":
                await HandleSetChannelCommand(Command);
                break;
            case "add-user":
                await HandleAddUserCommand(Command);
                break;
            case "remove-user":
                await HandleRemoveUserCommand(Command);
                break;
            case "list-users":
                await HandleListUsersCommand(Command);
                break;
            case "update-leaderboard":
                await HandleUpdateLeaderboardCommand(Command);
                break;
        }
    }

    #region Commands

    private async Task HandlePingCommand(SocketSlashCommand Command)
    {
        var embed = new EmbedBuilder();

        embed.AddField("Yes I'm alive, now bugger off", "Latency: How should I know???");

        await Command.RespondAsync(embed: embed.Build());
    }

    private async Task HandleSetChannelCommand(SocketSlashCommand Command)
    {
        Channel = Command.Data.Options.First().Value as IMessageChannel;

        // Write the new ChannelId to Channel.json
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Channel.Id, ChannelFilePath);

        // Also reset MessageId to avoid dumb crashed / force the bot to resend leaderboard
        LeaderboardMessageId = 0;

        // Write the new messageid to Message.json
        JsonInterface.WriteToJson(LeaderboardMessageId, MessageFilePath);

        await Command.RespondAsync($"Leaderboard will be sent in {Command.Data.Options.First().Value} from now on", ephemeral: true);
    }

    private async Task HandleAddUserCommand(SocketSlashCommand Command)
    {
        // Add inputet user to Users list
        Users.Add(Command.Data.Options.First().Value.ToString(), Command.Data.Options.ElementAt(1).Value.ToString());

        // Write to Users file
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Users, UsersFilePath);

        await Command.RespondAsync($"Added user {Command.Data.Options.First().Value} to leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task HandleRemoveUserCommand(SocketSlashCommand Command)
    {
        // Remove inputet user from Users list
        Users.Remove(Command.Data.Options.First().Value.ToString());

        // Write to Users file
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Users, UsersFilePath);

        await Command.RespondAsync($"Removed user {Command.Data.Options.First().Value} from leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task HandleListUsersCommand(SocketSlashCommand Command)
    {
        string Text = "";
        for (int i = 0; i < Users.Count; i++)
        {
            if (i == 0)
            {
                // If src and steam username are the same only write out once
                if (Users.ElementAt(i).Key == Users.ElementAt(i).Value)
                {
                    Text = $"{Users.ElementAt(i).Key}";
                }
                else
                {
                    Text = $"{Users.ElementAt(i).Key} | {Users.ElementAt(i).Value}";
                }
            }
            else
            {
                // If src and steam username are the same only write out once
                if (Users.ElementAt(i).Key == Users.ElementAt(i).Value)
                {
                    Text += $"\n{Users.ElementAt(i).Key}";
                }
                else
                {
                    Text += $"\n{Users.ElementAt(i).Key} | {Users.ElementAt(i).Value}";
                }
            }
        }

        if (Users.Count == 0)
        {
            Text = "No users are added";
        }

        var Embed = new EmbedBuilder();
        Embed.AddField("Users on leaderbaords", Text)
            .WithFooter("Usernames follow the structure:\n[speedrun.com] | [steam]\nIf only one name shows, they are the same");

        await Command.RespondAsync(embed: Embed.Build());
    }

    private async Task HandleUpdateLeaderboardCommand(SocketSlashCommand Command)
    {
        await Command.RespondAsync("Updating leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task GetPersonalBests()
    {
        List<ApiRequester.SrcResponse> RawSrcData = new();
        List<ApiRequester.BoardsResponse> RawBoardsData = new();
        ApiRequester.LpResponse RawLpData = new();

        // Get data for each user
        foreach (KeyValuePair<string, string> User in Users)
        {
            RawSrcData.Add(System.Text.Json.JsonSerializer.Deserialize<ApiRequester.SrcResponse>(ApiRequester.RequestData(0, User.Key)));
            RawBoardsData.Add(System.Text.Json.JsonSerializer.Deserialize<ApiRequester.BoardsResponse>(ApiRequester.RequestData(1, User.Value)));
        }

        // Then request LP data
        RawLpData = System.Text.Json.JsonSerializer.Deserialize<ApiRequester.LpResponse>(ApiRequester.RequestData(2, null));

        // Clean data to only keep the specific pbs we want to show
        List<CleanedResponse> NoSLA = new();
        List<CleanedResponse> Amc = new();
        List<CleanedResponse> Srm = new();
        List<CleanedResponse> Mel = new();
        List<CleanedResponse> Cm = new();
        List<CleanedResponse> SpLp = new();

        TimeCleaner TimeClean = new();

        // Clean and parse src runs
        for (int i = 0; i < RawSrcData.Count; i++)
        {
            for (int j = 0; j < RawSrcData[i].data.Count; j++)
            {
                // If game is Portal 2 and category is Singleplayer and it is NoSLA
                if (RawSrcData[i].data[j].run.game == "om1mw4d2" && RawSrcData[i].data[j].run.category == "jzd33ndn" && RawSrcData[i].data[j].run.values.sla == "z196dyy1")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    NoSLA.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
                }
                // If game is Portal 2 and category coop and it is Amc
                else if (RawSrcData[i].data[j].run.game == "om1mw4d2" && RawSrcData[i].data[j].run.category == "l9kv40kg" && RawSrcData[i].data[j].run.values.amc == "mln3x8nq")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    // Get second player
                    var client = new HttpClient();
                    string SecondPlayer = "";

                    foreach (ApiRequester.Player player in RawSrcData[i].data[j].run.players)
                    {
                        var response = await client.GetAsync(player.uri);

                        // If the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();

                            ApiRequester.SrcProfileResponse ProfileData = System.Text.Json.JsonSerializer.Deserialize<ApiRequester.SrcProfileResponse>(json, _readOptions);

                            // Check that we grabbed the other player
                            if (ProfileData.data.names.international != Users.ElementAt(i).Key)
                            {
                                SecondPlayer = ProfileData.data.names.international;
                                break;
                            }
                        }
                    }

                    Amc.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users.ElementAt(i).Key, Partner = SecondPlayer, Time = CleanTime });
                }
                // If game is Portal 2 Speedrun Mod and category is Single Player
                else if (RawSrcData[i].data[j].run.game == "lde3eme6" && RawSrcData[i].data[j].run.category == "ndx940vd")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    Srm.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
                }
                // If game is Portal Stories Mel and category is Story Mode and is Inbounds
                else if (RawSrcData[i].data[j].run.game == "j1nz9l1p" && RawSrcData[i].data[j].run.category == "q25oowgk" && RawSrcData[i].data[j].run.values.MelInbounds == "4lx8vp31")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    Mel.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
                }
            }
        }

        // Clean and parse CM runs
        CmMapParser MapParser = new();
        for (int i = 0; i < RawBoardsData.Count; i++)
        {
            try
            {
                Cm.Add(new CleanedResponse() { Runner = Users.ElementAt(i).Value, Place = int.Parse(RawBoardsData[i].times.SP.chambers.bestRank.scoreData.playerRank), Map = MapParser.ParseMap(RawBoardsData[i].times.SP.chambers.bestRank.map) });
            }
            catch
            {
                Console.WriteLine($"Failed to parse cm data for {Users.ElementAt(i).Value}");
            }
        }

        // Clean and parse LP scores
        for (int i = 0; i < RawLpData.data.Count; i++)
        {
            // Compare each added User with each user in lp.nekz.me to only save the Users added to this bot
            foreach (string User in Users.Values)
            {
                if (RawLpData.data[i].name == User)
                {
                    SpLp.Add(new CleanedResponse() { Runner = User, PortalCount = RawLpData.data[i].score, Place = RawLpData.data[i].rank }); 
                }
            }
        }

        // Sort pbs to find out who is first in each cat
        NoSLA = NoSLA.OrderBy(o => o.Place).ToList();
        Amc = Amc.OrderBy(o => o.Place).ToList();
        Srm = Srm.OrderBy(o => o.Place).ToList();
        Mel = Mel.OrderBy(o => o.Place).ToList();
        Cm = Cm.OrderBy(o => o.Place).ToList();
        SpLp = SpLp.OrderBy(o => o.PortalCount).ToList();

        // Clean all lists
        ResponseCleaner Cleaner = new();
        Cleaner.GetBestRunFromEachUser(NoSLA);
        Cleaner.GetBestRunFromEachUser(Amc);
        Cleaner.GetBestRunFromEachUser(Srm);
        Cleaner.GetBestRunFromEachUser(Mel);

        // Remove duplicates last as to not accidentaly remove faster runs (See issue #17)
        Cleaner.RemoveDuplicate(Amc);

        // Build the enbeded message
        var embed = new EmbedBuilder
        {
            Title = "Friendly Fire ON - Leaderboards"
        };

        // Build all text fields
        EmbedTextBuilder TextBuilder = new();

        embed.AddField("NoSLA",
            TextBuilder.BuildText(NoSLA, 0));

        embed.AddField("Amc",
            TextBuilder.BuildText(Amc, 1));

        embed.AddField("Speedrun Mod",
            TextBuilder.BuildText(Srm, 2));

        embed.AddField("Portal Stories: Mel",
            TextBuilder.BuildText(Mel, 3));

        embed.AddField("SP CM Best Place",
            TextBuilder.BuildText(Cm, 4));

        embed.AddField("SP Least Portals",
            TextBuilder.BuildText(SpLp, 5));

        embed.AddField("\u200B",
            $"Last updated {new TimestampTag(DateTimeOffset.UtcNow, TimestampTagStyles.Relative)}")
            // Add the footer to the last field
            .WithFooter(footer => footer.Text = $"To get added to the leaderboards, do /add-user");

        // If leaderboard doesnt exist, send it
        if (LeaderboardMessageId == 0)
        {
            IUserMessage LeaderboardMessage = await Channel.SendMessageAsync(embed: embed.Build());

            LeaderboardMessageId = LeaderboardMessage.Id;

            // Write the new messageid to Message.json
            JsonInterface JsonInterface = new();
            JsonInterface.WriteToJson(LeaderboardMessageId, MessageFilePath);
        }
        // Else, edit it
        else
        {
            await Channel.ModifyMessageAsync((ulong)LeaderboardMessageId, x =>
            {
                x.Embed = embed.Build();
            });
        }
    }

    #endregion
}
