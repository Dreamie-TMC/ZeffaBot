using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class BotInitializer
{
    internal DiscordSocketClient Client { get; set; }
    internal SeedHandler SeedHandler { get; set; }
    internal AboutHandler AboutHandler { get; set; }
    internal AsyncHandler AsyncHandler { get; set; }
    internal UpdateInfoHandler UpdateInfoHandler { get; set; }
    internal ScheduleAsyncsHandler ScheduleAsyncsHandler { get; set; }

    private Dictionary<ulong, Mutex> _guildUpdaterMutexes;

    public BotInitializer(DiscordSocketClient client,
        SeedHandler seedHandler, 
        AboutHandler aboutHandler, 
        AsyncHandler asyncHandler, 
        UpdateInfoHandler updateInfoHandler,
        ScheduleAsyncsHandler scheduleAsyncsHandler)
    {
        Client = client;
        SeedHandler = seedHandler;
        AboutHandler = aboutHandler;
        AsyncHandler = asyncHandler;
        UpdateInfoHandler = updateInfoHandler;
        ScheduleAsyncsHandler = scheduleAsyncsHandler;
        _guildUpdaterMutexes = new Dictionary<ulong, Mutex>();
    }

    public async Task RunInitializationCode()
    {
        var guilds = Client.Guilds;
        
        foreach (var guild in guilds)
        {
            await RunInitializationCodeOnGuild(guild);
        }
    }

    public async Task RunInitializationCodeOnGuild(SocketGuild guild)
    {
        var seedCommand = SeedHandler.BuildSlashCommand();
        var aboutCommand = AboutHandler.BuildSlashCommand();
        var asyncCommand = AsyncHandler.BuildSlashCommand();
        var updateCommand = UpdateInfoHandler.BuildSlashCommand();
        var scheduleAsyncsCommand = ScheduleAsyncsHandler.BuildSlashCommand();
        var removeSettingsCommand = ScheduleAsyncsHandler.BuildRemoveCommand();

        if (!_guildUpdaterMutexes.TryGetValue(guild.Id, out var mutex))
        {
            mutex = new Mutex();
            _guildUpdaterMutexes.Add(guild.Id, mutex);
        }
        
        Lock(mutex);
        
        var commands = await guild.GetApplicationCommandsAsync();

        try
        {
            IRole? asyncRole = null;
            if (!guild.Roles.Any(role => role.Name.Equals(Constants.AsyncRole)))
                asyncRole = await guild.CreateRoleAsync(Constants.AsyncRole);

            if (commands.All(command => command.Name != Constants.GenerateSeed))
                await guild.CreateApplicationCommandAsync(seedCommand.Build());
            if (commands.All(command => command.Name != Constants.AboutZeffa))
                await guild.CreateApplicationCommandAsync(aboutCommand.Build());
            if (commands.All(command => command.Name != Constants.GenerateAsync))
                await guild.CreateApplicationCommandAsync(asyncCommand.Build());
            if (commands.All(command => command.Name != Constants.ShowUpdateInfo))
                await guild.CreateApplicationCommandAsync(updateCommand.Build());
            if (commands.All(command => command.Name != Constants.SetupRegularAsyncs))
                await guild.CreateApplicationCommandAsync(scheduleAsyncsCommand.Build());
            if (commands.All(command => command.Name != Constants.RemoveAsyncConfig))
                await guild.CreateApplicationCommandAsync(removeSettingsCommand.Build());

            ulong categoryId = 0;

            if (guild.ForumChannels.All(channel => channel.Name != Constants.AsyncPostChannelName))
            {
                var category = guild.CategoryChannels.FirstOrDefault(category =>
                    category.Name.Equals(Constants.AsyncCategory, StringComparison.OrdinalIgnoreCase));

                if (category == null)
                    categoryId = (await guild.CreateCategoryChannelAsync(Constants.AsyncCategory,
                        props => { props.Position = guild.CategoryChannels.Count; })).Id;
                else
                    categoryId = category.Id;

                await guild.CreateForumChannelAsync(Constants.AsyncPostChannelName,
                    props =>
                    {
                        props.CategoryId = categoryId;
                        props.Topic = "Place for zeffa to post async seeds when they are generated.";
                    });
            }

            if (guild.TextChannels.All(channel => channel.Name != Constants.AutomaticAsyncSpoilerChannelName))
            {
                if (categoryId == 0)
                {
                    var category = guild.CategoryChannels.FirstOrDefault(category =>
                        category.Name.Equals(Constants.AsyncCategory, StringComparison.OrdinalIgnoreCase));

                    if (category == null)
                        categoryId = (await guild.CreateCategoryChannelAsync(Constants.AsyncCategory,
                            props => { props.Position = guild.CategoryChannels.Count; })).Id;
                    else
                        categoryId = category.Id;
                }

                var everyone = guild.Roles.First(role => role.IsEveryone);
                asyncRole ??= guild.Roles.First(role => role.Name == Constants.AsyncRole);
                var zeffaRole = guild.Users.First(user => user.Id == Client.CurrentUser.Id).Roles
                    .First(role => role.IsManaged);

                var channel = await guild.CreateTextChannelAsync(Constants.AutomaticAsyncSpoilerChannelName,
                    props =>
                    {
                        props.CategoryId = categoryId;
                        props.Topic =
                            "Place for zeffa to post spoilers. Only visible to her and people with the AsyncGenerator role.";
                    });

                await channel.AddPermissionOverwriteAsync(zeffaRole,
                    new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                await channel.AddPermissionOverwriteAsync(asyncRole,
                    new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny));
                await channel.AddPermissionOverwriteAsync(everyone,
                    new OverwritePermissions(viewChannel: PermValue.Deny, sendMessages: PermValue.Deny));
            }

            asyncRole ??= guild.Roles.First(role => role.Name == Constants.AsyncRole);

            var tempChannel =
                guild.TextChannels.FirstOrDefault(channel =>
                    channel.Name == Constants.AutomaticAsyncSpoilerChannelName &&
                    channel.GetPermissionOverwrite(asyncRole) == null);

            if (tempChannel != null)
            {
                await tempChannel.AddPermissionOverwriteAsync(asyncRole,
                    new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny));
            }
            Unlock(mutex);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create command in guild {guild.Name}! Please try again later.");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Unlock(mutex);
            throw;
        }
    }

    private void Lock(Mutex mutex)
    {
        mutex.WaitOne();
    }

    private void Unlock(Mutex mutex)
    {
        mutex.ReleaseMutex();
    }
}