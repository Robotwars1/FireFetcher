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
        public Times times { get; set; }
        public Values values { get; set; }
    }

    public class Times
    {
        public string primary { get; set; }
    }

    public class Values
    {
        [JsonPropertyName("9l7x7xzn")]
        public string sla { get; set; }
    }

    // Classes for cleaned data
    public class CleanedResponse
    {
        public string Runner { get; set; }
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
        // Read current Users list
        FileStream JsonFile = File.OpenRead(UsersFilePath);
        List<string> Users = System.Text.Json.JsonSerializer.Deserialize<List<string>>(JsonFile, _readOptions);
        JsonFile.Close();

        // Add inputet user to Users list
        Users.Add(command.Data.Options.First().Value.ToString());

        // Write to Users file
        JsonFile = File.OpenWrite(UsersFilePath);
        var Utf8JsonWriter = new Utf8JsonWriter(JsonFile);
        System.Text.Json.JsonSerializer.Serialize(Utf8JsonWriter, Users, _writeOptions);
        JsonFile.Close();

        await command.RespondAsync($"Added user {command.Data.Options.First().Value} to leaderboard", ephemeral: true);
    }

    private async Task HandleRemoveUserCommand(SocketSlashCommand command)
    {
        // Read current Users list
        FileStream JsonFile = File.OpenRead(UsersFilePath);
        List<string> Users = System.Text.Json.JsonSerializer.Deserialize<List<string>>(JsonFile, _readOptions);
        JsonFile.Close();

        // Remove inputet user from Users list
        Users.Remove(command.Data.Options.First().Value.ToString());

        // Write to Users file
        JsonFile = File.OpenWrite(UsersFilePath);
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
                    // Translate time
                    string Time = JsonData[i].data[j].run.times.primary.Replace("PT", "").Replace("H", ":").Replace("M", ":").Replace("S", "");

                    NoSLA.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users[i], Time = Time });
                }
                // If game is Portal 2 and category is Amc
                else if (JsonData[i].data[j].run.game == "om1mw4d2" && JsonData[i].data[j].run.category == "l9kv40kg")
                {
                    Amc.Add(new CleanedResponse() { Place = JsonData[i].data[j].place, Runner = Users[i] });
                }
            }
        }

        // Sort pbs to find out who is first in each cat
        NoSLA = NoSLA.OrderBy(o => o.Place).ToList();

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

        embed.AddField("NoSLA",
            sb.ToString());

        //embed.AddField("Amc",
        //    $"1st ");

        // Send leaderboard
        await Channel.SendMessageAsync(embed: embed.Build());
    }
}
