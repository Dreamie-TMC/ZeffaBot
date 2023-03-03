using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class GuildJoinHandler
{
    internal BotInitializer Initializer { get; set; }
    
    public GuildJoinHandler(BotInitializer initializer)
    {
        Initializer = initializer;
    }
    
    public async Task HandleGuildJoin(SocketGuild guild)
    {
        await Initializer.RunInitializationCode();
    }
}