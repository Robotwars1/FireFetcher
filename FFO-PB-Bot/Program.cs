using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient Client;

    string UsersFilePath = "Data/Users.json";

    private readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
        var guild = Client.GetGuild(628666811239497749);

        // Command for adding user to leaderboard
        var AddUserCommand = new SlashCommandBuilder()
            .WithName("add-user")
            .WithDescription("Adds a user to be included in the leaderboard updates")
            .AddOption("username", ApplicationCommandOptionType.String, "Speedrun.com username", isRequired: true);

        try
        {
            // Create each slash command
            await guild.CreateApplicationCommandAsync(AddUserCommand.Build());
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
            case "add-user":
                await HandleAddUserCommand(command);
                break;
        }
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
}
