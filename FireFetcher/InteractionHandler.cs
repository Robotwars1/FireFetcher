using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FireFetcher;
using System.Reflection;

namespace InteractionFramework;


// Original script form Discord.Net Github examples
// I don't really understand what it happening but it works
public class InteractionHandler
{
    private readonly DiscordSocketClient Client;
    private readonly InteractionService Handler;
    private readonly IServiceProvider Services;
    private readonly Program FireFetcher;
    private readonly UserHandler UserHandler;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, Program fireFetcher, UserHandler userHandler)
    {
        Client = client;
        Handler = handler;
        Services = services;
        FireFetcher = fireFetcher;
        UserHandler = userHandler;
    }

    public async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        Client.Ready += ReadyAsync;
        Handler.Log += LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await Handler.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += HandleInteraction;

        // Also process the result of the command execution.
        Handler.InteractionExecuted += HandleInteractionExecute;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        // Register the commands globally.
        // alternatively you can use _handler.RegisterCommandsGloballyAsync() to register commands to a specific guild.
        await Handler.RegisterCommandsGloballyAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(Client, interaction);

            // Execute the incoming command.
            var result = await Handler.ExecuteCommandAsync(context, Services);

            // Log the command used and who used it
            Logger Logger = new();
            Logger.CommandLog(context.User.Username);

            // Due to async nature of InteractionFramework, the result here may always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                default:
                    break;
            }

        return Task.CompletedTask;
    }
}
