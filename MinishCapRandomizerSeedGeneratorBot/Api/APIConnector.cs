using Discord;
using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class APIConnector
{    
    internal DiscordSocketClient Client { get; set; }
    internal BotInitializer Initializer { get; set; }
    internal SlashCommandHandler SlashHandler { get; set; }
    internal GuildEventHandler GuildEventHandler { get; set; }
    internal AppSettings AppSettings { get; set; }

    public APIConnector()
    {
    }

    public APIConnector(DiscordSocketClient client,
        BotInitializer initializer,
        SlashCommandHandler handler, 
        GuildEventHandler guildEventHandler,
        AppSettings settings)
    {
        Client = client;
        Initializer = initializer;
        SlashHandler = handler;
        GuildEventHandler = guildEventHandler;
        AppSettings = settings;
    }
    
    public async Task BuildSocketClient()
    {
        Client.Log += Log;

        await Client.LoginAsync(TokenType.Bot, AppSettings.DiscordAccessToken);
        Client.Ready += ClientReady;
        Client.SlashCommandExecuted += SlashHandler.HandleSlashCommands;
        Client.JoinedGuild += GuildEventHandler.HandleGuildJoin;
        Client.LeftGuild += GuildEventHandler.HandleGuildLeave;
        Client.ChannelDestroyed += GuildEventHandler.HandleChannelDestroyed;
        Client.RoleDeleted += GuildEventHandler.HandleRoleDeleted;
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