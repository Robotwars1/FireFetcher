using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// Thingy to call other classes (in other .cs files)
using FireFetcher;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient Client;

    IMessageChannel Channel;
    ulong LeaderboardMessageId;
    Dictionary<string, string> Users = new();

    // Paths to each .json file
    const string UsersFilePath = "Data/Users.json";
    const string MessageFilePath = "Data/Message.json";
    const string ChannelFilePath = "Data/Channel.json";

    private readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public class SrcResponse
    {
        public List<Data> data { get; set; }
    }

    public class Data
    {
        public int place { get; set; }
        public Run run { get; set; }
    }

    public class Run
    {
        public string game { get; set; }
        public string category { get; set; }
        public List<Player> players { get; set; }
        public Times times { get; set; }
        public Values values { get; set; }
    }

    public class Player
    {
        public Uri uri { get; set; }
    }

    public class Times
    {
        public string primary { get; set; }
    }

    public class Values
    {
        [JsonPropertyName("9l7x7xzn")]
        public string sla { get; set; }
        [JsonPropertyName("38dj54e8")]
        public string amc { get; set; }
        [JsonPropertyName("wl333p9l")]
        public string MelInbounds { get; set; }
    }

    public class SrcProfileResponse
    {
        public ProfileData data { get; set; }
    }

    public class ProfileData
    {
        public Names names { get; set; }
    }

    public class Names
    {
        public string international { get; set; }
    }

    public class BoardsResponse
    {
        public BoardsTimes times { get; set; }
    }

    public class BoardsTimes
    {
        public SP SP { get; set; }
    }

    public class SP
    {
        public Chambers chambers { get; set; }
    }

    public class Chambers
    {
        public BestRank bestRank { get; set; }
    }

    public class BestRank
    {
        public ScoreData scoreData { get; set; }
        public object map { get; set; }
    }

    public class ScoreData
    {
        public string playerRank { get; set; }
    }

    public class LpResponse
    {
        public List<LpData> data { get; set; }
    }

    public class LpData
    {
        public string name { get; set; }
        public int score { get; set; }
        public int rank { get; set; }
    }

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

        // Read current Users list
        FileStream JsonFile = File.OpenRead(UsersFilePath);
        try
        {
            Users = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(JsonFile, _readOptions);
        }
        catch
        {
            Console.WriteLine("Failed to get users from Users.json");
        }
        JsonFile.Close();

        // Read message id
        JsonFile = File.OpenRead(MessageFilePath);
        try
        {
            LeaderboardMessageId = System.Text.Json.JsonSerializer.Deserialize<ulong>(JsonFile, _readOptions);
        }
        catch
        {
            Console.WriteLine("Failed to get id from Message.json");
        }
        JsonFile.Close();

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

        // Read channel id
        // Gets channel id later than everything else cause it doesnt work otherwise ¯\_(ツ)_/¯
        FileStream JsonFile = File.OpenRead(ChannelFilePath);
        try
        {
            Channel = Client.GetChannel(System.Text.Json.JsonSerializer.Deserialize<ulong>(JsonFile, _readOptions)) as IMessageChannel;
        }
        catch
        {
            Console.WriteLine("Failed to get channel from Channel.json");
        }
        JsonFile.Close();
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        Logger Logger = new();
        Logger.CommandLog(command.Data.Name, command.User.ToString());

        switch (command.Data.Name)
        {
            case "ping":
                await HandlePingCommand(command);
                break;
            case "set-channel":
                await HandleSetChannelCommand(command);
                break;
            case "add-user":
                await HandleAddUserCommand(command);
                break;
            case "remove-user":
                await HandleRemoveUserCommand(command);
                break;
            case "list-users":
                await HandleListUsersCommand(command);
                break;
            case "update-leaderboard":
                await HandleUpdateLeaderboardCommand(command);
                break;
        }
    }

    private async Task HandlePingCommand(SocketSlashCommand command)
    {
        var embed = new EmbedBuilder();

        embed.AddField("Yes I'm alive, now bugger off", "Latency: How should I know???");

        await command.RespondAsync(embed: embed.Build());
    }

    private async Task HandleSetChannelCommand(SocketSlashCommand command)
    {
        Channel = command.Data.Options.First().Value as IMessageChannel;

        // Write the new ChannelId to Channel.json
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Channel.Id, ChannelFilePath);

        Logger Logger = new();
        Logger.JsonLog(Channel.Id.ToString(), ChannelFilePath);

        // Also reset MessageId to avoid dumb crashed / force the bot to resend leaderboard
        LeaderboardMessageId = 0;

        // Write the new messageid to Message.json
        JsonInterface.WriteToJson(LeaderboardMessageId, MessageFilePath);

        Logger.JsonLog(LeaderboardMessageId.ToString(), MessageFilePath);

        await command.RespondAsync($"Leaderboard will be sent in {command.Data.Options.First().Value} from now on", ephemeral: true);
    }

    private async Task HandleAddUserCommand(SocketSlashCommand command)
    {
        // Add inputet user to Users list
        Users.Add(command.Data.Options.First().Value.ToString(), command.Data.Options.ElementAt(1).Value.ToString());

        // Write to Users file
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Users, UsersFilePath);

        Logger Logger = new();
        Logger.JsonLog(Users.ToString(), UsersFilePath);

        await command.RespondAsync($"Added user {command.Data.Options.First().Value} to leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task HandleRemoveUserCommand(SocketSlashCommand command)
    {
        // Remove inputet user from Users list
        Users.Remove(command.Data.Options.First().Value.ToString());

        // Write to Users file
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Users, UsersFilePath);

        Logger Logger = new();
        Logger.JsonLog(Users.ToString(), UsersFilePath);

        await command.RespondAsync($"Removed user {command.Data.Options.First().Value} from leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task HandleListUsersCommand(SocketSlashCommand command)
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

        await command.RespondAsync(embed: Embed.Build());
    }

    private async Task HandleUpdateLeaderboardCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Updating leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task GetPersonalBests()
    {
        List<SrcResponse> JsonData = new();
        List<BoardsResponse> RawBoardsData = new();

        // Itterate through speedrun.com usernames
        foreach (string User in Users.Keys)
        {
            // Get speedrun.com pbs
            var client = new HttpClient();

            // Create the Url to where needed data is gathered
            UriBuilder UriBuilder = new();
            UriBuilder.Scheme = "http";
            UriBuilder.Host = "www.speedrun.com";
            UriBuilder.Path = "api/v1/users/";
            UriBuilder.Path += $"{User}/";
            UriBuilder.Path += "personal-bests";
            string Url = UriBuilder.ToString();

            var response = await client.GetAsync(Url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                // Parse the contents as a .json
                SrcResponse TempDataHolder = System.Text.Json.JsonSerializer.Deserialize<SrcResponse>(json, _readOptions);
                JsonData.Add(TempDataHolder);
            }
        }

        // Itterate through steam usernames
        foreach (string User in Users.Values)
        {
            // Get board.portal2.sr pbs
            var client = new HttpClient();

            UriBuilder UriBuilder = new();
            UriBuilder.Scheme = "http";
            UriBuilder.Host = "board.portal2.sr";
            UriBuilder.Path = "profile/";
            UriBuilder.Path += $"{User}/";
            UriBuilder.Path += "json";
            string Url = UriBuilder.ToString();

            var response = await client.GetAsync(Url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                // Parse the contents as a .json
                BoardsResponse TempHolder = System.Text.Json.JsonSerializer.Deserialize<BoardsResponse>(json, _readOptions);
                RawBoardsData.Add(TempHolder);
            }
        }

        // Get lp.nekz.me pbs
        var LpClient = new HttpClient();

        string LpUrl = "https://lp.nekz.me/api/v1/sp";

        var LpResponse = await LpClient.GetAsync(LpUrl);

        LpResponse RawLpResponse = new();

        if (LpResponse.IsSuccessStatusCode)
        {
            var json = await LpResponse.Content.ReadAsStringAsync();

            // Parse the contents as a .json
            RawLpResponse = System.Text.Json.JsonSerializer.Deserialize<LpResponse>(json, _readOptions);
        }

        // Clean data to only keep the specific pbs we want to show
        List<CleanedResponse> NoSLA = new();
        List<CleanedResponse> Amc = new();
        List<CleanedResponse> Srm = new();
        List<CleanedResponse> Mel = new();

        List<CleanedResponse> Cm = new();

        List<CleanedResponse> SpLp = new();

        TimeCleaner TimeClean = new();

        // Clean and parse src runs
        for (int i = 0; i < JsonData.Count; i++)
        {
            for (int j = 0; j < JsonData[i].data.Count; j++)
            {
                // If game is Portal 2 and category is Singleplayer and it is NoSLA
                if (JsonData[i].data[j].run.game == "om1mw4d2" && JsonData[i].data[j].run.category == "jzd33ndn" && JsonData[i].data[j].run.values.sla == "z196dyy1")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(JsonData[i].data[j].run.times.primary);

                    NoSLA.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
                }
                // If game is Portal 2 and category coop and it is Amc
                else if (JsonData[i].data[j].run.game == "om1mw4d2" && JsonData[i].data[j].run.category == "l9kv40kg" && JsonData[i].data[j].run.values.amc == "mln3x8nq")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(JsonData[i].data[j].run.times.primary);

                    // Get second player
                    var client = new HttpClient();
                    string SecondPlayer = "";

                    foreach (Player player in JsonData[i].data[j].run.players)
                    {
                        var response = await client.GetAsync(player.uri);

                        // If the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();

                            SrcProfileResponse ProfileData = System.Text.Json.JsonSerializer.Deserialize<SrcProfileResponse>(json, _readOptions);

                            // Check that we grabbed the other player
                            if (ProfileData.data.names.international != Users.ElementAt(i).Key)
                            {
                                SecondPlayer = ProfileData.data.names.international;
                                break;
                            }
                        }
                    }

                    Amc.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users.ElementAt(i).Key, Partner = SecondPlayer, Time = CleanTime });
                }
                // If game is Portal 2 Speedrun Mod and category is Single Player
                else if (JsonData[i].data[j].run.game == "lde3eme6" && JsonData[i].data[j].run.category == "ndx940vd")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(JsonData[i].data[j].run.times.primary);

                    Srm.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
                }
                // If game is Portal Stories Mel and category is Story Mode and is Inbounds
                else if (JsonData[i].data[j].run.game == "j1nz9l1p" && JsonData[i].data[j].run.category == "q25oowgk" && JsonData[i].data[j].run.values.MelInbounds == "4lx8vp31")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(JsonData[i].data[j].run.times.primary);

                    Mel.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users.ElementAt(i).Key, Time = CleanTime });
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
        for (int i = 0; i < RawLpResponse.data.Count; i++)
        {
            foreach (string User in Users.Values)
            {
                if (RawLpResponse.data[i].name == User)
                {
                    SpLp.Add(new CleanedResponse() { Runner = User, PortalCount = RawLpResponse.data[i].score, Place = RawLpResponse.data[i].rank }); 
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
        Cleaner.RemoveDuplicate(Amc);

        Cleaner.GetBestRunFromEachUser(NoSLA);
        Cleaner.GetBestRunFromEachUser(Amc);
        Cleaner.GetBestRunFromEachUser(Srm);
        Cleaner.GetBestRunFromEachUser(Mel);

        // Build the enbeded message
        var embed = new EmbedBuilder
        {
            Title = "Friendly Fire ON - Leaderboards"
        };

        // Build all text fields
        EmbedTextBuilder TextBuilder = new();

        embed.AddField("NoSLA",
            TextBuilder.BuildText(NoSLA, true, false, false));

        embed.AddField("Amc",
            TextBuilder.BuildText(Amc, false, false, false));

        embed.AddField("Speedrun Mod",
            TextBuilder.BuildText(Srm, true, false, false));

        embed.AddField("Portal Stories: Mel",
            TextBuilder.BuildText(Mel, true, false, false));

        embed.AddField("SP CM Best Place",
            TextBuilder.BuildText(Cm, true, true, false));

        embed.AddField("SP Least Portals",
            TextBuilder.BuildText(SpLp, true, false, true));

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

            Logger Logger = new();
            Logger.JsonLog(LeaderboardMessageId.ToString(), MessageFilePath);
        }
        // Else, edit it
        else
        {
            await Channel.ModifyMessageAsync(LeaderboardMessageId, x =>
            {
                x.Embed = embed.Build();
            });
        }
    }
}
