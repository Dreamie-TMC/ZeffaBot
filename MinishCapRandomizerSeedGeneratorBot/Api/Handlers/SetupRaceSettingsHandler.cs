using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Guild.Models;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class SetupRaceSettingsHandler
{
    internal static readonly string SettingsString = "settings-string";
    
    internal GuildSettings GuildSettings { get; set; }
    internal DiscordSocketClient SocketClient { get; set; }

    public SetupRaceSettingsHandler(GuildSettings guildSettings, DiscordSocketClient socketClient)
    {
        GuildSettings = guildSettings;
        SocketClient = socketClient;
    }

    public async Task HandleSetupRaceSettingsCommand(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue)
        {
            await command.RespondAsync("You cannot use this command here!", ephemeral: true);
            return;
        }

        var guildUser = SocketClient.Guilds.First(guild => guild.Id == command.GuildId.Value).Users
            .First(user => user.Id == command.User.Id);
        
        if (!guildUser.Roles.Any(role => role.Name.Equals(Constants.RaceModeratorRole, StringComparison.OrdinalIgnoreCase)))
        {
            await command.RespondAsync("You are not authorized to run this command!", ephemeral: true);
            return;
        }

        if (command.Data.Options.First().Value is not string settingsString)
        {
            await command.RespondAsync("Failed to parse settings string! String is required!", ephemeral: true);
            return;
        }
        
        GuildSettings.AddGuildToConfig(command.GuildId.Value);
        
        GuildSettings.UpdateGuildConfig(new RaceConfig
        {
            SettingsString = settingsString,
        }, command.GuildId.Value);
        
        GuildSettings.WriteConfigOutput();

        await command.RespondAsync("Configuration saved successfully!", ephemeral: true);
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.SetupRaceSettings)
            .WithDescription("Sets the race settings used by /generate-race-seed for this server.")
            .AddOption(SettingsString, ApplicationCommandOptionType.String, "The settings string to use for races.",
                true);
    }
}