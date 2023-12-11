using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient Client;
    IMessageChannel Channel;
    IUserMessage LeaderboardMessage;

    List<string> Users = new();

    string UsersFilePath = "Data/Users.json";

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

    // Classes for cleaned data
    public class CleanedResponse
    {
        public string Runner { get; set; }
        public string Partner { get; set; }
        public int Place { get; set; }
        public string Time { get; set; }
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
        Users = System.Text.Json.JsonSerializer.Deserialize<List<string>>(JsonFile, _readOptions);
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
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task Client_Ready()
    {
        // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
        var guild = Client.GetGuild(1004897099017637979);

        // Command for setting which server channel to send leaderboard in
        var SetChannelCommand = new SlashCommandBuilder()
            .WithName("set-channel")
            .WithDescription("Sets which channel to send leaderboard in")
            .AddOption("channel", ApplicationCommandOptionType.Channel, "Channel", isRequired: true);

        // Command for adding user to leaderboard
        var AddUserCommand = new SlashCommandBuilder()
            .WithName("add-user")
            .WithDescription("Adds a user to be included in the leaderboard updates")
            .AddOption("username", ApplicationCommandOptionType.String, "Speedrun.com username", isRequired: true);

        // Command for removing user from leaderboard
        var RemoveUserCommand = new SlashCommandBuilder()
            .WithName("remove-user")
            .WithDescription("Removes a user from the leaderboard")
            .AddOption("username", ApplicationCommandOptionType.String, "Speedrun.com username", isRequired: true);

        // Command for updating leaderboard
        var UpdateLeaderboardCommand = new SlashCommandBuilder()
            .WithName("update-leaderboard")
            .WithDescription("Forces an update of the leaderboard");

        try
        {
            // Create each slash command
            await guild.CreateApplicationCommandAsync(SetChannelCommand.Build());
            await guild.CreateApplicationCommandAsync(AddUserCommand.Build());
            await guild.CreateApplicationCommandAsync(RemoveUserCommand.Build());
            await guild.CreateApplicationCommandAsync(UpdateLeaderboardCommand.Build());
        }
        catch (HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "set-channel":
                await HandleSetChannelCommand(command);
                break;
            case "add-user":
                await HandleAddUserCommand(command);
                break;
            case "remove-user":
                await HandleRemoveUserCommand(command);
                break;
            case "update-leaderboard":
                await HandleUpdateLeaderboardCommand(command);
                break;
        }
    }

    private async Task HandleSetChannelCommand(SocketSlashCommand command)
    {
        Channel = command.Data.Options.First().Value as IMessageChannel;

        await command.RespondAsync($"Leaderboard will be sent in {command.Data.Options.First().Value} from now on", ephemeral: true);
    }

    private async Task HandleAddUserCommand(SocketSlashCommand command)
    {
        // Add inputet user to Users list
        Users.Add(command.Data.Options.First().Value.ToString());

        // Write to Users file
        FileStream JsonFile = File.Create(UsersFilePath);
        var Utf8JsonWriter = new Utf8JsonWriter(JsonFile);
        System.Text.Json.JsonSerializer.Serialize(Utf8JsonWriter, Users, _writeOptions);
        JsonFile.Close();

        await command.RespondAsync($"Added user {command.Data.Options.First().Value} to leaderboard", ephemeral: true);
    }

    private async Task HandleRemoveUserCommand(SocketSlashCommand command)
    {
        // Remove inputet user from Users list
        Users.Remove(command.Data.Options.First().Value.ToString());

        // Write to Users file
        FileStream JsonFile = File.Create(UsersFilePath);
        var Utf8JsonWriter = new Utf8JsonWriter(JsonFile);
        System.Text.Json.JsonSerializer.Serialize(Utf8JsonWriter, Users, _writeOptions);
        JsonFile.Close();

        await command.RespondAsync($"Removed user {command.Data.Options.First().Value} from leaderboard", ephemeral: true);
    }

    private async Task HandleUpdateLeaderboardCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Updating leaderboard", ephemeral: true);

        await GetPersonalBests();
    }

    private async Task GetPersonalBests()
    {
        List<SrcResponse> JsonData = new();

        // Get users
        FileStream JsonFile = File.OpenRead(UsersFilePath);
        List<string> Users = System.Text.Json.JsonSerializer.Deserialize<List<string>>(JsonFile, _readOptions);
        JsonFile.Close();

        foreach (string User in Users)
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

            // If the response is successful, we'll 
            // interpret the response as XML 
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                // We can then use the LINQ to XML API to query the XML 
                SrcResponse TempDataHolder = System.Text.Json.JsonSerializer.Deserialize<SrcResponse>(json, _readOptions);
                JsonData.Add(TempDataHolder);
            }



            // Get board.portal2.sr pbs
        }

        // Clean data to only keep the specific pbs we want to show
        List<CleanedResponse> NoSLA = new();
        List<CleanedResponse> Amc = new();

        for (int i = 0; i < JsonData.Count; i++)
        {
            for (int j = 0; j < JsonData[i].data.Count; j++)
            {
                // If game is Portal 2 and category is Singleplayer and it is NoSLA
                if (JsonData[i].data[j].run.game == "om1mw4d2" && JsonData[i].data[j].run.category == "jzd33ndn" && JsonData[i].data[j].run.values.sla == "z196dyy1")
                {
                    string DirtyTime = JsonData[i].data[j].run.times.primary;

                    StringBuilder StringBuild = new();

                    string Hour = "";
                    string Minute = "";
                    string Second = "";
                    string MiliSecond = "";

                    int CharIndex = 0;

                    foreach (char Char in DirtyTime)
                    {
                        StringBuild.Append(Char);

                        if (StringBuild.ToString().EndsWith("H"))
                        {
                            Hour = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith("M"))
                        {
                            Minute = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith(".") || CharIndex == DirtyTime.Length)
                        {
                            Second = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith("S"))
                        {
                            MiliSecond = StringBuild.ToString();
                            StringBuild = new();
                        }

                        CharIndex++;
                    }

                    // Check if Minute and Second are missing a 0 in front (basicly if they are less than 10
                    if (Minute.Length == 2)
                    {
                        Minute = Minute.Insert(0, "0");
                    }
                    if (Second.Length == 2)
                    {
                        Second = Second.Insert(0, "0");
                    }

                    DirtyTime = "" + Hour + Minute + Second + MiliSecond;

                    // Translate time
                    string CleanTime = DirtyTime.Replace("PT", "").Replace("H", ":").Replace("M", ":").Replace("S", "");

                    NoSLA.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users[i], Time = CleanTime });
                }
                // If game is Portal 2 and category coop and it is Amc
                else if (JsonData[i].data[j].run.game == "om1mw4d2" && JsonData[i].data[j].run.category == "l9kv40kg" && JsonData[i].data[j].run.values.amc == "mln3x8nq")
                {
                    string DirtyTime = JsonData[i].data[j].run.times.primary;

                    StringBuilder StringBuild = new();

                    string Hour = "";
                    string Minute = "";
                    string Second = "";
                    string MiliSecond = "";

                    int CharIndex = 0;

                    foreach (char Char in DirtyTime)
                    {
                        StringBuild.Append(Char);

                        if (StringBuild.ToString().EndsWith("H"))
                        {
                            Hour = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith("M"))
                        {
                            Minute = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith(".") || CharIndex == DirtyTime.Length)
                        {
                            Second = StringBuild.ToString();
                            StringBuild = new();
                        }
                        else if (StringBuild.ToString().EndsWith("S"))
                        {
                            MiliSecond = StringBuild.ToString();
                            StringBuild = new();
                        }

                        CharIndex++;
                    }

                    // Check if Minute and Second are missing a 0 in front (basicly if they are less than 10
                    if (Minute.Length == 2)
                    {
                        Minute = Minute.Insert(0, "0");
                    }
                    if (Second.Length == 2)
                    {
                        Second = Second.Insert(0, "0");
                    }

                    DirtyTime = "" + Hour + Minute + Second + MiliSecond;

                    // Translate time
                    string CleanTime = DirtyTime.Replace("PT", "").Replace("H", ":").Replace("M", ":").Replace("S", "");

                    // Get second player
                    var client = new HttpClient();
                    string SecondPlayer = "";

                    foreach (Player player in JsonData[i].data[j].run.players)
                    {
                        var response = await client.GetAsync(player.uri);

                        // If the response is successful, we'll 
                        // interpret the response as XML 
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();

                            // We can then use the LINQ to XML API to query the XML 
                            SrcProfileResponse ProfileData = System.Text.Json.JsonSerializer.Deserialize<SrcProfileResponse>(json, _readOptions);
                            
                            // Check that we grabbed the other player
                            if (ProfileData.data.names.international != Users[i])
                            {
                                SecondPlayer = ProfileData.data.names.international;
                                break;
                            }
                        }
                    }

                    Amc.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users[i], Partner = SecondPlayer, Time = CleanTime });
                }
            }
        }

        // Sort pbs to find out who is first in each cat
        NoSLA = NoSLA.OrderBy(o => o.Place).ToList();
        Amc = Amc.OrderBy(o => o.Place).ToList();

        List<int> IndexToRemove = new();

        for (int i = 0; i < Amc.Count; i++)
        {
            for (int j = 0; j < Amc.Count; j++)
            {
                // Dont compare a run with itself && dont compare with runs that should already be removed
                if (i != j && !IndexToRemove.Contains(j) && !IndexToRemove.Contains(i))
                {
                    // Check there are no exact duplicates in Amc (with both ways to flip users)
                    if ((Amc[i].Runner == Amc[j].Partner && Amc[i].Partner == Amc[j].Runner) || (Amc[i].Runner == Amc[j].Runner && Amc[i].Partner == Amc[j].Partner))
                    {
                        IndexToRemove.Add(j);
                    }
                    // Check one Runner doesnt have 2 runs in Amc
                    if (Amc[i].Runner == Amc[j].Runner)
                    {
                        // This keeps the first instance of said runner, eg their fastest time
                        IndexToRemove.Add(j);
                    }
                }
            }
        }

        // Remove duplicate entries
        IndexToRemove = IndexToRemove.Distinct().ToList();

        // Removing backwards to avoid errors
        IndexToRemove = IndexToRemove.OrderByDescending(o => o).ToList();
        foreach (int Index in IndexToRemove)
        {
            Amc.RemoveAt(Index);
        }

        // Build the enbeded message
        var embed = new EmbedBuilder
        {
            // Embed property can be set within object initializer
            Title = "Friendly Fire ON - Leaderboards",
        };

        // Build NoSLA Text
        StringBuilder sb = new("");
        for (int i = 0; i < NoSLA.Count; i++)
        {
            switch (i)
            {
                case 0:
                    sb.Append($"1st - {NoSLA[i].Runner} - {NoSLA[i].Time}");
                    break;
                case 1:
                    sb.Append($"\n2nd - {NoSLA[i].Runner} - {NoSLA[i].Time}");
                    break;
                case 2:
                    sb.Append($"\n3rd - {NoSLA[i].Runner} - {NoSLA[i].Time}");
                    break;
                case > 2:
                    sb.Append($"\n{i + 1}th - {NoSLA[i].Runner} - {NoSLA[i].Time}");
                    break;
            }
        }

        if (NoSLA.Count == 0)
        {
            sb.Append("No runs available");
        }

        embed.AddField("NoSLA",
            sb.ToString());

        // Build Amc Text
        sb = new("");
        for (int i = 0; i < Amc.Count; i++)
        {
            switch (i)
            {
                case 0:
                    sb.Append($"1st - {Amc[i].Runner} & {Amc[i].Partner} - {Amc[i].Time}");
                    break;
                case 1:
                    sb.Append($"\n2nd - {Amc[i].Runner} & {Amc[i].Partner} - {Amc[i].Time}");
                    break;
                case 2:
                    sb.Append($"\n3rd - {Amc[i].Runner} & {Amc[i].Partner} - {Amc[i].Time}");
                    break;
                case > 2:
                    sb.Append($"\n{i + 1}th - {Amc[i].Runner} & {Amc[i].Partner} - {Amc[i].Time}");
                    break;
            }
        }

        if (Amc.Count == 0)
        {
            sb.Append("No runs available");
        }

        embed.AddField("Amc",
            sb.ToString());

        // If leaderboard doesnt exist, send it
        if (LeaderboardMessage == null)
        {
            LeaderboardMessage = await Channel.SendMessageAsync(embed: embed.Build());
        }
        // Else, edit it
        else
        {
            await LeaderboardMessage.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
            });
        }
    }
}
