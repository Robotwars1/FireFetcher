using Discord;
using Discord.WebSocket;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;

// Thingy to call other classes (in other .cs files)
using FireFetcher;
using Discord.Interactions;
using InteractionFramework;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private readonly JsonInterface JsonInterface = new();
    private readonly ApiRequester ApiRequester = new();

    private readonly DiscordSocketConfig Config = new()
    {
        GatewayIntents = GatewayIntents.None
    };

    private static IServiceProvider Services;
    private DiscordSocketClient Client;

    IMessageChannel Channel;
    ulong? LeaderboardMessageId;

    int LastHour = DateTime.Now.Hour;

    // Paths to each .json file
    const string MessageFilePath = "Data/Message.json";
    const string ChannelFilePath = "Data/Channel.json";

    private readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #region speedrun.com

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

    #endregion

    #region boards.portal2.sr

    public class BoardsResponse
    {
        public BoardsPoints points { get; set; }
    }

    public class BoardsPoints
    {
        public SP SP { get; set; }
    }

    public class SP
    {
        public int score { get; set; }
    }

    #endregion

    #region lp.nekz.me

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

    #endregion

    public class Username
    {
        public ulong DiscordID { get; set; }
        public string DiscordName { get; set; } = string.Empty;
        public string SpeedrunCom { get; set; } = string.Empty;
        public string BoardProfileID { get; set; } = string.Empty;
        public string Steam { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
    }

    // Classes for cleaned data
    public class CleanedResponse
    {
        public string Runner { get; set; }
        public string RunnerNickname { get; set; }
        public string Partner { get; set; }
        public int Place { get; set; }
        public string Time { get; set; }
        public int Points { get; set; }
        public int PortalCount { get; set; }
    }

    public async Task MainAsync()
    {
        Services = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton(this)
            .AddSingleton(new UserHandler())
            .BuildServiceProvider();

        Services.GetRequiredService<UserHandler>().Setup(this);

        Client = Services.GetRequiredService<DiscordSocketClient>();

        // Setup logging
        Client.Log += Log;

        // Here we can initialize the service that will register and execute our commands
        await Services.GetRequiredService<InteractionHandler>().InitializeAsync();

        // Read bot-token
        // DO NOT MAKE TOKEN PUBLIC
        var token = File.ReadAllText("Token.txt");

        // Start bot
        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        // Hooking up more commands
        Client.Ready += ReadSavedData;

        // Setup the timer for updating leaderboards every hour
        System.Timers.Timer HourTimer = new System.Timers.Timer(60000); // Call function every minute
        HourTimer.Elapsed += new ElapsedEventHandler(CheckHour);
        HourTimer.Start();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Logger Logger = new();
        Logger.GeneralLog(msg.Message);

        return Task.CompletedTask;
    }

    public async Task ReadSavedData()
    {
        // Read saved data
        LeaderboardMessageId = (ulong?)JsonInterface.ReadJson(MessageFilePath, "ID");
        // If LeaderboardMessageId returns null, set it to base value (0)
        if (LeaderboardMessageId == null)
        {
            LeaderboardMessageId = 0;
        }
        ulong? ChannelId = (ulong?)JsonInterface.ReadJson(ChannelFilePath, "ID");
        if (ChannelId != null)
        {
            Channel = Client.GetChannel((ulong)ChannelId) as IMessageChannel;
        }
    }

    public async Task SetChannel(ITextChannel channel)
    {
        Channel = channel;

        // Write the new ChannelId to Channel.json
        JsonInterface JsonInterface = new();
        JsonInterface.WriteToJson(Channel.Id, ChannelFilePath);

        // Also reset MessageId to avoid dumb crashed / force the bot to resend leaderboard
        LeaderboardMessageId = 0;

        // Write the new messageid to Message.json
        JsonInterface.WriteToJson(LeaderboardMessageId, MessageFilePath);
    }

    public async Task CreateLeaderboard()
    {
        List<Username> Users = Services.GetRequiredService<UserHandler>().Users;

        // If Channel isnt set then we cant send leaderboard and return early
        if (Channel == null)
        {
            return;
        }

        List<SrcResponse> RawSrcData = new();
        List<BoardsResponse> RawBoardsData = new();
        LpResponse RawLpData = new();

        // Get data for each user
        for (int i = 0; i < Users.Count; i++)
        {
            RawSrcData.Add(JsonSerializer.Deserialize<SrcResponse>(ApiRequester.RequestData(0, Users[i].SpeedrunCom).Result, _readOptions));
            RawBoardsData.Add(JsonSerializer.Deserialize<BoardsResponse>(ApiRequester.RequestData(1, Users[i].BoardProfileID).Result, _readOptions));
        }

        // Then request LP data
        RawLpData = JsonSerializer.Deserialize<LpResponse>(ApiRequester.RequestData(2, null).Result, _readOptions);

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

                    NoSLA.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users[i].SpeedrunCom, RunnerNickname = Users[i].Nickname, Time = CleanTime });
                }
                // If game is Portal 2 and category coop and it is Amc
                else if (RawSrcData[i].data[j].run.game == "om1mw4d2" && RawSrcData[i].data[j].run.category == "l9kv40kg" && RawSrcData[i].data[j].run.values.amc == "mln3x8nq")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    // Get second player
                    var client = new HttpClient();
                    string SecondPlayer = "";

                    foreach (Player player in RawSrcData[i].data[j].run.players)
                    {
                        var response = await client.GetAsync(player.uri);

                        // If the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();

                            SrcProfileResponse ProfileData = JsonSerializer.Deserialize<SrcProfileResponse>(json, _readOptions);

                            // Check that we grabbed the other player (Ignore capitalization)
                            if (!string.Equals(ProfileData.data.names.international, Users[i].SpeedrunCom, StringComparison.OrdinalIgnoreCase))
                            {
                                SecondPlayer = ProfileData.data.names.international;
                                break;
                            }
                        }
                    }

                    Amc.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users[i].SpeedrunCom, RunnerNickname = Users[i].Nickname, Partner = SecondPlayer, Time = CleanTime });
                }
                // If game is Portal 2 Speedrun Mod and category is Single Player
                else if (RawSrcData[i].data[j].run.game == "lde3eme6" && RawSrcData[i].data[j].run.category == "ndx940vd")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    Srm.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users[i].SpeedrunCom, RunnerNickname = Users[i].Nickname, Time = CleanTime });
                }
                // If game is Portal Stories Mel and category is Story Mode and is Inbounds
                else if (RawSrcData[i].data[j].run.game == "j1nz9l1p" && RawSrcData[i].data[j].run.category == "q25oowgk" && RawSrcData[i].data[j].run.values.MelInbounds == "4lx8vp31")
                {
                    // Call function to clean the time
                    string CleanTime = TimeClean.Clean(RawSrcData[i].data[j].run.times.primary);

                    Mel.Add(new CleanedResponse() { Place = RawSrcData[i].data[j].place, Runner = Users[i].SpeedrunCom, RunnerNickname = Users[i].Nickname, Time = CleanTime });
                }
            }
        }

        // Clean and parse CM runs
        CmMapParser MapParser = new();
        for (int i = 0; i < RawBoardsData.Count; i++)
        {
            try
            {
                Cm.Add(new CleanedResponse() { Runner = Users[i].Steam, RunnerNickname = Users[i].Nickname, Points = RawBoardsData[i].points.SP.score, Place = i + 1 });
            }
            catch
            {
                Console.WriteLine($"Failed to parse cm data for {Users[i].Steam}");
            }
        }

        // Clean and parse LP scores
        for (int i = 0; i < RawLpData.data.Count; i++)
        {
            // Compare each added User with each user in lp.nekz.me to only save the Users added to this bot
            for (int j = 0; j < Users.Count; j++)
            {
                if (string.Equals(RawLpData.data[i].name, Users[j].Steam, StringComparison.OrdinalIgnoreCase))
                {
                    SpLp.Add(new CleanedResponse() { Runner = Users[j].Steam, RunnerNickname = Users[j].Nickname, PortalCount = RawLpData.data[i].score, Place = RawLpData.data[i].rank }); 
                }
            }
        }

        // Sort pbs to find out who is first in each cat
        NoSLA = NoSLA.OrderBy(o => o.Place).ToList();
        Amc = Amc.OrderBy(o => o.Place).ToList();
        Srm = Srm.OrderBy(o => o.Place).ToList();
        Mel = Mel.OrderBy(o => o.Place).ToList();
        Cm = Cm.OrderByDescending(o => o.Points).ToList();
        SpLp = SpLp.OrderBy(o => o.PortalCount).ToList();

        // Clean all lists
        ListCleaner Cleaner = new();
        Cleaner.GetBestRunFromEachUser(NoSLA);
        Cleaner.GetBestRunFromEachUser(Amc);
        Cleaner.GetBestRunFromEachUser(Srm);
        Cleaner.GetBestRunFromEachUser(Mel);

        // Remove duplicates last as to not accidentaly remove faster runs (See issue #17)
        Cleaner.RemoveDuplicate(Amc);

        // Build the enbeded message
        var Embed = new EmbedBuilder
        {
            Title = "Friendly Fire ON - Leaderboards"
        };

        // Build all text fields
        EmbedTextBuilder TextBuilder = new();
        Embed.AddField("NoSLA", TextBuilder.BuildText(NoSLA, 0));
        Embed.AddField("Amc", TextBuilder.BuildText(Amc, 1));
        Embed.AddField("Speedrun Mod", TextBuilder.BuildText(Srm, 2));
        Embed.AddField("Portal Stories: Mel", TextBuilder.BuildText(Mel, 3));
        Embed.AddField("SP CM Points", TextBuilder.BuildText(Cm, 4));
        Embed.AddField("SP Least Portals", TextBuilder.BuildText(SpLp, 5));
        Embed.AddField("\u200B", $"Last updated {new TimestampTag(DateTimeOffset.UtcNow, TimestampTagStyles.Relative)}")
            // Add the footer to the last field
            .WithFooter(footer => footer.Text = $"To get added to the leaderboards, do /link-accounts");

        // If leaderboard doesnt exist, send it
        if (LeaderboardMessageId == 0)
        {
            IUserMessage LeaderboardMessage = await Channel.SendMessageAsync(embed: Embed.Build());

            LeaderboardMessageId = LeaderboardMessage.Id;

            // Write the new messageid to Message.json
            JsonInterface JsonInterface = new();
            JsonInterface.WriteToJson(LeaderboardMessageId.ToString(), MessageFilePath);
        }
        // Else, edit it
        else
        {
            await Channel.ModifyMessageAsync((ulong)LeaderboardMessageId, x =>
            {
                x.Embed = Embed.Build();
            });
        }
    }

    private void CheckHour(object source, ElapsedEventArgs e)
    {
        // If an hour has passed, update leaderboards
        if (LastHour < DateTime.Now.Hour || (LastHour == 23 && DateTime.Now.Hour == 0))
        {
            LastHour = DateTime.Now.Hour;
            CreateLeaderboard();
        }
    }
}
