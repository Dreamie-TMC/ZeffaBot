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

    public BotInitializer(DiscordSocketClient client,
        SeedHandler seedHandler, 
        AboutHandler aboutHandler, 
        AsyncHandler asyncHandler, 
        UpdateInfoHandler updateInfoHandler)
    {
        Client = client;
        SeedHandler = seedHandler;
        AboutHandler = aboutHandler;
        AsyncHandler = asyncHandler;
        UpdateInfoHandler = updateInfoHandler;
    }

    public async Task RunInitializationCode()
    {
        var guilds = Client.Guilds;
        
        var seedCommand = SeedHandler.BuildSlashCommand();
        var aboutCommand = AboutHandler.BuildSlashCommand();
        var asyncCommand = AsyncHandler.BuildSlashCommand();
        var updateCommand = UpdateInfoHandler.BuildSlashCommand();
        
        foreach (var guild in guilds)
        {
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
                
                if (guild.ForumChannels.All(channel => channel.Name != Constants.AsyncPostChannelName))
                {
                    var category = guild.CategoryChannels.FirstOrDefault(category =>
                        category.Name.Equals(Constants.AsyncCategory, StringComparison.OrdinalIgnoreCase));

                    ulong categoryId;

                    if (category == null)
                        categoryId = (await guild.CreateCategoryChannelAsync(Constants.AsyncCategory, props =>
                        {
                            props.Position = guild.CategoryChannels.Count;
                        })).Id;
                    else
                        categoryId = category.Id;
                    
                    await guild.CreateForumChannelAsync(Constants.AsyncPostChannelName,
                        props =>
                        {
                            props.CategoryId = categoryId;
                            props.Topic = "Place for zeffa to post async seeds when they are generated.";
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create command in guild {guild.Name}! Please try again later.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }
    }
}