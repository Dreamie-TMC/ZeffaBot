using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Api.Handlers;
using MinishCapRandomizerSeedGeneratorBot.Threading;

namespace MinishCapRandomizerSeedGeneratorBot.Api;

public class BotInitializer
{
    internal DiscordSocketClient Client { get; set; }
    internal SeedHandler SeedHandler { get; set; }
    internal AboutHandler AboutHandler { get; set; }
    internal AsyncHandler AsyncHandler { get; set; }
    internal UpdateInfoHandler UpdateInfoHandler { get; set; }
    internal ScheduleAsyncsHandler ScheduleAsyncsHandler { get; set; }
    internal GenerateRaceSeedHandler GenerateRaceSeedHandler { get; set; }
    internal SetupRaceSettingsHandler SetupRaceSettingsHandler { get; set; }

    private SynchronizedList<ulong> GuildsCurrentlyBeingUpdated { get; set; }

    public BotInitializer(DiscordSocketClient client,
        SeedHandler seedHandler, 
        AboutHandler aboutHandler, 
        AsyncHandler asyncHandler, 
        UpdateInfoHandler updateInfoHandler,
        ScheduleAsyncsHandler scheduleAsyncsHandler,
        SetupRaceSettingsHandler setupRaceSettingsHandler,
        GenerateRaceSeedHandler generateRaceSeedHandler)
    {
        Client = client;
        SeedHandler = seedHandler;
        AboutHandler = aboutHandler;
        AsyncHandler = asyncHandler;
        UpdateInfoHandler = updateInfoHandler;
        ScheduleAsyncsHandler = scheduleAsyncsHandler;
        SetupRaceSettingsHandler = setupRaceSettingsHandler;
        GenerateRaceSeedHandler = generateRaceSeedHandler;
        GuildsCurrentlyBeingUpdated = new SynchronizedList<ulong>();
    }

    public async Task RunInitializationCode()
    {
        var guilds = Client.Guilds;
        
        foreach (var guild in guilds)
        {
            new Thread(async () => await RunInitializationCodeOnGuild(guild)).Start();
        }
    }

    public async Task RunInitializationCodeOnGuild(SocketGuild guild)
    {
        if (GuildsCurrentlyBeingUpdated.Contains(guild.Id))
            return;
        
        GuildsCurrentlyBeingUpdated.Add(guild.Id);
        
        var generateSeedCommand = SeedHandler.BuildSlashCommand();
        var showAboutInfoCommand = AboutHandler.BuildSlashCommand();
        var generateAsyncCommand = AsyncHandler.BuildSlashCommand();
        var showUpdateInfoCommand = UpdateInfoHandler.BuildSlashCommand();
        var scheduleAsyncsCommand = ScheduleAsyncsHandler.BuildSlashCommand();
        var removeAsyncConfigCommand = ScheduleAsyncsHandler.BuildRemoveCommand();
        var setupRaceSettingsCommand = SetupRaceSettingsHandler.BuildSlashCommand();
        var generateRaceSeedCommand = GenerateRaceSeedHandler.BuildSlashCommand();

        var commands = await guild.GetApplicationCommandsAsync();

        try
        {
            IRole? asyncRole = null;
            if (!guild.Roles.Any(role => role.Name.Equals(Constants.AsyncRole)))
                asyncRole = await guild.CreateRoleAsync(Constants.AsyncRole);
            if (!guild.Roles.Any(role => role.Name.Equals(Constants.RaceModeratorRole)))
                await guild.CreateRoleAsync(Constants.RaceModeratorRole);

            var command = commands.FirstOrDefault(command => command.Name != Constants.GenerateSeed);
            if (command == null || command.Options.Count != generateSeedCommand.Options.Count)
                await guild.CreateApplicationCommandAsync(generateSeedCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.AboutZeffa);
            if (command == null)
                await guild.CreateApplicationCommandAsync(showAboutInfoCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.GenerateAsync);
            if (command == null || command.Options.Count != generateAsyncCommand.Options.Count)
                await guild.CreateApplicationCommandAsync(generateAsyncCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.ShowUpdateInfo);
            if (command == null)
                await guild.CreateApplicationCommandAsync(showUpdateInfoCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.SetupRegularAsyncs);
            if (command == null || command.Options.Count != scheduleAsyncsCommand.Options.Count)
                await guild.CreateApplicationCommandAsync(scheduleAsyncsCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.RemoveAsyncConfig);
            if (command == null)
                await guild.CreateApplicationCommandAsync(removeAsyncConfigCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.SetupRaceSettings);
            if (command == null || command.Options.Count != setupRaceSettingsCommand.Options.Count)
                await guild.CreateApplicationCommandAsync(setupRaceSettingsCommand.Build());
            
            command = commands.FirstOrDefault(command => command.Name != Constants.GenerateRaceSeed);
            if (command == null || command.Options.Count != generateRaceSeedCommand.Options.Count)
                await guild.CreateApplicationCommandAsync(generateRaceSeedCommand.Build());

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
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to update guild {guild.Name}! Please try again later.");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            GuildsCurrentlyBeingUpdated.Remove(guild.Id);
        }
    }
}