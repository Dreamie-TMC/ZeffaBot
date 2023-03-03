using System.Text;
using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Models;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Controllers;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class SeedHandler
{
    internal static readonly string Seed = "seed";
    internal static readonly string UseSettingPreset = "use-setting-preset";
    internal static readonly string SettingsString = "settings-string";
    internal static readonly string CosmeticsString = "cosmetics-string";
    internal static readonly string UploadSpoiler = "upload-spoiler-log";
    internal static readonly string ShowSeedNumber = "show-seed-number";
    internal static readonly string OnlyShowYouSeed = "only-show-you-seed";

    internal string DefaultSettings { get; set; }
    internal string DefaultCosmetics { get; set; }
    internal ShufflerController ShufflerController { get; set; }
    internal AppSettings AppSettings { get; set; }
    internal SynchronizedQueue Queue { get; set; }
    internal PresetHandler PresetHandler { get; set; }

    public SeedHandler(ShufflerController shufflerController, AppSettings appSettings, SynchronizedQueue queue, PresetHandler presetHandler)
    {
        ShufflerController = shufflerController;
        AppSettings = appSettings;
        Queue = queue;
        PresetHandler = presetHandler;
        
        shufflerController.LoadRom(AppSettings.RomPath);
        shufflerController.LoadLogicFile();

        DefaultSettings = shufflerController.GetSettingsString();
        DefaultCosmetics = shufflerController.GetCosmeticsString();
    }

    public async Task HandleGenerateSeedCommand(SocketSlashCommand command)
    {
        var parameters = command.Data.Options;
        var seed = parameters.FirstOrDefault(param => param.Name == Seed);
        var settingsString = parameters.FirstOrDefault(param => param.Name == SettingsString);
        var preset = parameters.FirstOrDefault(param => param.Name == UseSettingPreset);
        var cosmeticsString = parameters.FirstOrDefault(param => param.Name == CosmeticsString);
        var uploadSpoiler = parameters.FirstOrDefault(param => param.Name == UploadSpoiler);
        var showSeed = parameters.FirstOrDefault(param => param.Name == ShowSeedNumber);
        var onlyShowInvoker = parameters.FirstOrDefault(param => param.Name == OnlyShowYouSeed);

        var seedNum = seed == null ? new Random().Next() : (int)((long)seed.Value & int.MaxValue);
        while (seedNum == 0) seedNum = new Random().Next();

        var settings = preset == null ? 
            settingsString == null ? "" : (string)settingsString.Value 
            : (string)preset.Value;
        var cosmetics = cosmeticsString == null ? "" : (string)cosmeticsString.Value;

        var spoiler = uploadSpoiler == null || (bool)uploadSpoiler.Value;
        var showSeedInfo = showSeed == null || (bool)showSeed.Value;
        var onlyShowInvokerTheSeed = onlyShowInvoker != null && (bool)onlyShowInvoker.Value;

        var request = new SeedGenerationRequest
        {
            Command = command,
            CosmeticsString = cosmetics,
            SettingsString = settings,
            OnlyRespondToCaller = onlyShowInvokerTheSeed,
            Seed = seedNum,
            ShowSeedInfoInResponse = showSeedInfo,
            UploadSpoiler = spoiler
        };

        Queue.Enqueue(request);

        await command.RespondAsync($"You are number {Queue.GetQueueCount()} in the queue!", ephemeral: true);
    }

    internal SlashCommandBuilder BuildSlashCommand()
    {
        var presets = PresetHandler.Presets;
        return new SlashCommandBuilder()
            .WithName(Constants.GenerateSeed)
            .WithDescription("Generates a seed and uploads the patch if generation succeeded.")
            .AddOption(Seed, ApplicationCommandOptionType.Integer, "The seed number to use for generation.", isRequired: false)
            .AddOption(BuildOptionForPresets())
            .AddOption(SettingsString, ApplicationCommandOptionType.String, "The settings string to use for generation.", isRequired: false)
            .AddOption(CosmeticsString, ApplicationCommandOptionType.String, "The cosmetics string to use for generation.", isRequired: false)
            .AddOption(UploadSpoiler, ApplicationCommandOptionType.Boolean, "Choose whether the spoiler is also uploaded with the patch.", isRequired: false)
            .AddOption(ShowSeedNumber, ApplicationCommandOptionType.Boolean, "Choose whether to show the seed info in the message at the end.", isRequired: false)
            .AddOption(OnlyShowYouSeed, ApplicationCommandOptionType.Boolean, "Choose whether to show the seed only to you or to everyone. Defaults to false.", isRequired: false);
    }

    private SlashCommandOptionBuilder BuildOptionForPresets()
    {
        var builder = new SlashCommandOptionBuilder()
            .WithName(UseSettingPreset)
            .WithDescription("The setting preset to use for generation. Overwrites provided setting string.")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.String);

        return PresetHandler.Presets.Aggregate(builder, (current, preset) => current.AddChoice(preset.PresetName, preset.PresetString));
    }
}