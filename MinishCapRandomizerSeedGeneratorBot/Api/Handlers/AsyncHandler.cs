using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Models;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Random;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class AsyncHandler
{
    internal static readonly string Count = "num-seeds";
    internal static readonly string UseSettingPreset = "use-setting-preset";
    internal static readonly string SettingsString = "settings-string";
    internal static readonly string CosmeticsString = "cosmetics-string";
    
    internal DiscordSocketClient SocketClient { get; set; }
    internal SynchronizedQueue Queue { get; set; }
    internal PresetHandler PresetHandler { get; set; }

    public AsyncHandler(DiscordSocketClient client, SynchronizedQueue queue, PresetHandler presetHandler)
    {
        SocketClient = client;
        Queue = queue;
        PresetHandler = presetHandler;
    }
    
    public async Task HandleGenerateAsyncCommand(SocketSlashCommand command)
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

        await command.RespondAsync("Queueing async seeds for generation!", ephemeral: true);

        var thread = new Thread(async () => await AddSeedsToQueue(command));
        thread.Start();
    }

    private async Task AddSeedsToQueue(SocketSlashCommand command)
    {
        var parameters = command.Data.Options;

        var seedsToGenerate = (int)((long)parameters.First(param => param.Name == Count).Value & int.MaxValue);

        if (seedsToGenerate > 5)
            seedsToGenerate = 5;
        if (seedsToGenerate < 1)
            seedsToGenerate = 1;

        for (var i = 0; i < seedsToGenerate; ++i)
        {
            var preset = parameters.FirstOrDefault(param => param.Name == $"{UseSettingPreset}-{i + 1}");
            var settingsString = parameters.FirstOrDefault(param => param.Name == $"{SettingsString}-{i + 1}");
            var cosmeticsString = parameters.FirstOrDefault(param => param.Name == $"{CosmeticsString}-{i + 1}");

            var seed = new SquaresRandomNumberGenerator().Next();
            var settings = preset == null ? 
                settingsString == null ? "" : (string)settingsString.Value 
                : (string)preset.Value;
            var cosmetics = cosmeticsString == null ? "" : (string)cosmeticsString.Value;

            var request = new SeedGenerationRequest
            {
                Command = command,
                CosmeticsString = cosmetics,
                SettingsString = settings,
                OnlyRespondToCaller = false,
                Seed = seed,
                ShowSeedInfoInResponse = false,
                UploadSpoiler = false,
                IsAsync = true,
                AsyncSeedNumber = i + 1,
                GuildNumber = command.GuildId!.Value,
                IsAutomatedGeneration = false,
            };

            Queue.Enqueue(request);
        }

        await command.FollowupAsync("Seeds queued!", ephemeral: true);
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.GenerateAsync)
            .WithDescription("Generates a series of async seeds and creates threads for each seed.")
            .AddOption(BuildSeedCommand())
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