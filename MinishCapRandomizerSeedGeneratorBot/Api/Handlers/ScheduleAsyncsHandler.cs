using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Guild.Models;
using MinishCapRandomizerSeedGeneratorBot.Models;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class ScheduleAsyncsHandler
{
    internal static readonly string Count = "num-seeds";
    internal static readonly string DayToGenerate = "day-to-generate";
    internal static readonly string UseSettingPreset = "use-setting-preset";
    internal static readonly string SettingsString = "settings-string";
    internal static readonly string CosmeticsString = "cosmetics-string";
    
    internal GuildSettings GuildSettings { get; set; }
    internal DiscordSocketClient SocketClient { get; set; }
    internal PresetHandler PresetHandler { get; set; }
    
    public ScheduleAsyncsHandler(GuildSettings guildSettings, DiscordSocketClient socketClient, PresetHandler presetHandler)
    {
        GuildSettings = guildSettings;
        SocketClient = socketClient;
        PresetHandler = presetHandler;
    }

    public async Task HandleSetupScheduledAsyncsCommand(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue || !command.ChannelId.HasValue)
        {
            await command.RespondAsync("Cannot run this command here!", ephemeral: true);
            return;
        }

        var guildUser = SocketClient.Guilds.First(guild => guild.Id == command.GuildId.Value).Users
            .First(user => user.Id == command.User.Id);

        if (!guildUser.Roles.Any(role => role.Name.Equals(Constants.AsyncRole, StringComparison.OrdinalIgnoreCase)))
        {
            await command.RespondAsync("You are not authorized to run this command!", ephemeral: true);
            return;
        }
        
        await command.RespondAsync("Saving config...", ephemeral: true);
        
        var parameters = command.Data.Options;
        
        GuildSettings.AddGuildToConfig(command.GuildId.Value);

        var seedsToGenerate = (int)((long)parameters.First(param => param.Name == Count).Value & int.MaxValue);
        var dayOfWeek = Enum.Parse<DayOfWeek>((string)parameters.First(param => param.Name == DayToGenerate).Value);

        if (seedsToGenerate > 5)
            seedsToGenerate = 5;
        if (seedsToGenerate < 1)
            seedsToGenerate = 1;

        var asyncConfig = new AsyncConfig
        {
            AsyncGenerationDayOfWeek = dayOfWeek,
            SupportsWeeklyAutomaticAsyncGeneration = true,
            TotalSeedsToGenerate = seedsToGenerate,
        };

        var asyncStrings = new List<AsyncStrings>();

        for (var i = 0; i < seedsToGenerate; ++i)
        {
            var preset = parameters.FirstOrDefault(param => param.Name == $"{UseSettingPreset}-{i + 1}");
            var settingsString = parameters.FirstOrDefault(param => param.Name == $"{SettingsString}-{i + 1}");
            var cosmeticsString = parameters.FirstOrDefault(param => param.Name == $"{CosmeticsString}-{i + 1}");

            var settings = preset == null ? 
                settingsString == null ? "" : (string)settingsString.Value 
                : (string)preset.Value;
            var cosmetics = cosmeticsString == null ? "" : (string)cosmeticsString.Value;

            asyncStrings.Add(new AsyncStrings
            {
                CosmeticsString = cosmetics,
                SeedNumber = i + 1,
                SettingsString = settings,
            });
        }

        asyncConfig.AsyncGenerationSettingAndCosmeticStrings = asyncStrings;

        GuildSettings.UpdateGuildConfig(asyncConfig, command.GuildId.Value);
        GuildSettings.WriteConfigOutput();
        await command.FollowupAsync("Configuration saved!", ephemeral: true);
    }

    public async Task HandleRemoveConfigCommand(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue || !command.ChannelId.HasValue)
        {
            await command.RespondAsync("Cannot run this command here!", ephemeral: true);
            return;
        }

        var guildUser = SocketClient.Guilds.First(guild => guild.Id == command.GuildId.Value).Users
            .First(user => user.Id == command.User.Id);

        if (!guildUser.Roles.Any(role => role.Name.Equals(Constants.AsyncRole, StringComparison.OrdinalIgnoreCase)))
        {
            await command.RespondAsync("You are not authorized to run this command!", ephemeral: true);
            return;
        }
        
        GuildSettings.RemoveGuildFromConfig(command.GuildId.Value);
        GuildSettings.WriteConfigOutput();
        await command.RespondAsync("Guild async configuration removed!", ephemeral: true);
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.SetupRegularAsyncs)
            .WithDescription("Generates a series of async seeds and creates threads for each seed.")
            .AddOption(BuildSeedCommand())
            .AddOption(BuildDayOfWeekCommand())
            .AddOption(BuildOptionForPresets(1))
            .AddOption(BuildOption(SettingsString, "settings", 1))
            .AddOption(BuildOption(CosmeticsString, "cosmetics", 1))
            .AddOption(BuildOptionForPresets(2))
            .AddOption(BuildOption(SettingsString, "settings", 2))
            .AddOption(BuildOption(CosmeticsString, "cosmetics", 2))
            .AddOption(BuildOptionForPresets(3))
            .AddOption(BuildOption(SettingsString, "settings", 3))
            .AddOption(BuildOption(CosmeticsString, "cosmetics", 3))
            .AddOption(BuildOptionForPresets(4))
            .AddOption(BuildOption(SettingsString, "settings", 4))
            .AddOption(BuildOption(CosmeticsString, "cosmetics", 4))
            .AddOption(BuildOptionForPresets(5))
            .AddOption(BuildOption(SettingsString, "settings", 5))
            .AddOption(BuildOption(CosmeticsString, "cosmetics", 5));
    }

    internal SlashCommandBuilder BuildRemoveCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.RemoveAsyncConfig)
            .WithDescription("Removes weekly async config for this guild.");
    }
    
    private static SlashCommandOptionBuilder BuildSeedCommand()
    {
        return new SlashCommandOptionBuilder()
            .WithName(Count)
            .WithDescription("The number of async seeds to generate.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Integer)
            .AddChoice("1", 1)
            .AddChoice("2", 2)
            .AddChoice("3", 3)
            .AddChoice("4", 4)
            .AddChoice("5", 5);
    }

    private static SlashCommandOptionBuilder BuildDayOfWeekCommand()
    {
        return new SlashCommandOptionBuilder()
            .WithName(DayToGenerate)
            .WithDescription("The day of the week to generate the seeds on.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String)
            .AddChoice("Sunday", "Sunday")
            .AddChoice("Monday", "Monday")
            .AddChoice("Tuesday", "Tuesday")
            .AddChoice("Wednesday", "Wednesday")
            .AddChoice("Thursday", "Thursday")
            .AddChoice("Friday", "Friday")
            .AddChoice("Saturday", "Saturday");
    }

    private SlashCommandOptionBuilder BuildOptionForPresets(int seedNum)
    {
        var builder = new SlashCommandOptionBuilder()
            .WithName($"{UseSettingPreset}-{seedNum}")
            .WithDescription($"The setting preset to use for seed {seedNum}. Overwrites provided setting string for seed {seedNum}.")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.String);

        return PresetHandler.Presets.Aggregate(builder, (current, preset) => current.AddChoice(preset.PresetName, preset.PresetString));
    }

    private SlashCommandOptionBuilder BuildOption(string optionName, string settingName, int seedNum)
    {
        return new SlashCommandOptionBuilder()
            .WithName($"{optionName}-{seedNum}")
            .WithDescription($"The {settingName} string to use for seed {seedNum}. Leave blank for default.")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.String);
    }
}