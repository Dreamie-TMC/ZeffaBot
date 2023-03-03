using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Threading;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class GuildEventHandler
{
    internal BotInitializer Initializer { get; set; }
    internal GuildSettings GuildSettings { get; set; }
    internal ScheduledEventDispatcher ScheduledEventDispatcher { get; set; }
    
    public GuildEventHandler(BotInitializer initializer, GuildSettings guildSettings, ScheduledEventDispatcher scheduledEventDispatcher)
    {
        Initializer = initializer;
        GuildSettings = guildSettings;
        ScheduledEventDispatcher = scheduledEventDispatcher;
    }
    
    public async Task HandleGuildJoin(SocketGuild guild)
    {
        new Thread(async () => await Initializer.RunInitializationCodeOnGuild(guild)).Start();
    }
    
    public async Task HandleGuildLeave(SocketGuild guild)
    {
        GuildSettings.RemoveGuildFromConfig(guild.Id);
        GuildSettings.WriteConfigOutput();
        ScheduledEventDispatcher.SetGuildCountUpdated();
    }

    public async Task HandleChannelDestroyed(SocketChannel channel)
    {
        switch (channel)
        {
            case SocketForumChannel forumChannel:
            {
                if (forumChannel.Name == Constants.AsyncPostChannelName)
                    new Thread(async () => await Initializer.RunInitializationCodeOnGuild(forumChannel.Guild)).Start();
                break;
            }
            case SocketTextChannel textChannel:
            {
                if (textChannel.Name == Constants.AutomaticAsyncSpoilerChannelName)
                    new Thread(async () => await Initializer.RunInitializationCodeOnGuild(textChannel.Guild)).Start();
                break;
            }
        }
    }

    public async Task HandleRoleDeleted(SocketRole role)
    {
        if (role.Name == Constants.AsyncRole)
            await Initializer.RunInitializationCodeOnGuild(role.Guild);
    }
}