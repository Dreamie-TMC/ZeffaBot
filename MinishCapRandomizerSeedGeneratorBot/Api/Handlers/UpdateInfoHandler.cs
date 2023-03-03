using Discord;
using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class UpdateInfoHandler
{
    public UpdateInfoHandler()
    { }

    public async Task HandleUpdateInfoCommand(SocketSlashCommand command)
    {
        await command.RespondAsync(@"Zeffa bot has been upgraded! Here are the newest things added or addressed in the update!
- Added two new commands for scheduling weekly Async seeds! These are only usable by people with the correct role.
- Added a private channel to post automatically generated spoilers to.
- Added support for using presets instead of only strings to all seed generation commands.
Here are the things in the previous update!
- Zeffa got access to the four sword and used it to clone herself and make 4 of her! This means she can now process seeds from multiple users at once and in multiple servers!
- Generating long seeds no longer closes communication with discord causing the seed to fail.
- Added two new commands, one for pushing the latest updates (the command you are running right now!) and one for generating async seeds! The async seed generation requires a special role to use.
- Added a new role for handling generating async seeds.
- ~~Replaced Zeffa with a robot clone because birds aren't real~~
- ||I just wanted to spoiler this text to see if you clicked it :heart: ty for clicking it||
- Zeffa creates her required channels, roles, and commands automatically when she is added to a server, and only does it once.");
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.ShowUpdateInfo)
            .WithDescription("Publishes information about the last update to Zeffa!.");
    }
}