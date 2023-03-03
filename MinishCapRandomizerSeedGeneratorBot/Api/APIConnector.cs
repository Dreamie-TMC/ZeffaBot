using Discord;
using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class APIConnector
{    
    internal DiscordSocketClient Client { get; set; }
    internal BotInitializer Initializer { get; set; }
    internal SlashCommandHandler SlashHandler { get; set; }
    internal GuildJoinHandler GuildJoinHandler { get; set; }
    internal AppSettings AppSettings { get; set; }

    public APIConnector()
    {
    }

    public APIConnector(DiscordSocketClient client,
        BotInitializer initializer,
        SlashCommandHandler handler, 
        GuildJoinHandler guildJoinHandler,
        AppSettings settings)
    {
        Client = client;
        Initializer = initializer;
        SlashHandler = handler;
        GuildJoinHandler = guildJoinHandler;
        AppSettings = settings;
    }
    
    public async Task BuildSocketClient()
    {
        Client.Log += Log;

        await Client.LoginAsync(TokenType.Bot, AppSettings.DiscordAccessToken);
        Client.Ready += ClientReady;
        Client.SlashCommandExecuted += SlashHandler.HandleSlashCommands;
        Client.JoinedGuild += GuildJoinHandler.HandleGuildJoin;
        await Client.StartAsync();
    }
    
    public async Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
    }

    private async Task ClientReady()
    {
        await Initializer.RunInitializationCode();
    }
}