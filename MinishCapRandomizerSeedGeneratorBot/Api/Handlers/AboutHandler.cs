using Discord;
using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class AboutHandler
{

    internal async Task HandleAboutCommand(SocketSlashCommand command)
    {
        await command.RespondAsync(
            "Hello there! I'm Zeffa, the adorable bird from the Minish Cap who flies Link all over the place! What's this? A talking bird? We live in a world with magical fairies and evil sorcerers and the thing you question is a talking bird???? Oh well, anyway, it is nice to meet you!\n\nYou can use me to generate seeds for TMCR! Check out my commands in the command list!\n\nI am currently hosted on Hailey's laptop, so i cannot guarantee 100% uptime :sob: :sob: :sob: but I'm going to try my hardest to be available all hours of the day! :blush: :purple_heart:");
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.AboutZeffa)
            .WithDescription("Gets information about the best birb, Zeffa!");
    }
}