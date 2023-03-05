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
- Added two new commands for scheduling weekly Async seeds! These are only usable by people with the AsyncGenerator role.
- Added a 'Race Moderator' role. Users with this role can configure race setting for the server.
- Added two new commands for setting up race settings and generating race seeds! Settings are saved for each server. Setting up settings requires the Race Moderator role. This means that you can now just use the command /generate-race-seed to generate seeds on the current race settings!
- Added a private channel to post automatically generated async seed spoilers to.
- Added support for using settings presets instead of only settings strings to all seed generation commands.
- ||Thank you for clicking the spoiler, it warms my heart every time :heart:||");
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.ShowUpdateInfo)
            .WithDescription("Publishes information about the last update to Zeffa!.");
    }
}