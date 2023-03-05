using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Controllers;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class GenerateRaceSeedHandler
{
    internal static readonly string IsSeedForRace = "is-seed-for-race";
    internal static readonly string Seed = "seed";
    internal static readonly string CosmeticsString = "cosmetics-string";
    internal static readonly string UploadSpoiler = "upload-spoiler-log";
    internal static readonly string ShowSeedNumber = "show-seed-number";
    internal static readonly string OnlyShowYouSeed = "only-show-you-seed";
    
    internal ShufflerController ShufflerController { get; set; }
    internal SynchronizedQueue Queue { get; set; }
    internal GuildSettings GuildSettings { get; set; }
    
    public GenerateRaceSeedHandler(ShufflerController shufflerController, SynchronizedQueue queue, GuildSettings guildSettings)
    {
        GuildSettings = guildSettings;
        ShufflerController = shufflerController;
        Queue = queue;
    }

    public async Task HandleGenerateRaceSeedCommand(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue)
        {
            await command.RespondAsync("You cannot use this command here!", ephemeral: true);
            return;
        }

        var guildConfig = GuildSettings.GetGuildFromGuildId(command.GuildId.Value);

        if (guildConfig == null || string.IsNullOrWhiteSpace(guildConfig.RaceConfig.SettingsString))
        {
            await command.RespondAsync("No race configuration exists for this server!", ephemeral: true);
            return;
        }
        
        var parameters = command.Data.Options;
        var isRaceSeed = (bool)parameters.First(param => param.Name == IsSeedForRace).Value;
        var seed = parameters.FirstOrDefault(param => param.Name == Seed);
        var cosmeticsString = parameters.FirstOrDefault(param => param.Name == CosmeticsString);
        var uploadSpoiler = parameters.FirstOrDefault(param => param.Name == UploadSpoiler);
        var showSeed = parameters.FirstOrDefault(param => param.Name == ShowSeedNumber);
        var onlyShowInvoker = parameters.FirstOrDefault(param => param.Name == OnlyShowYouSeed);

        var seedNum = seed == null ? new Random().Next() : (int)((long)seed.Value & int.MaxValue);
        while (seedNum == 0) seedNum = new Random().Next();
        
        var cosmetics = cosmeticsString == null ? "" : (string)cosmeticsString.Value;

        var spoiler = uploadSpoiler == null || (bool)uploadSpoiler.Value;
        var showSeedInfo = showSeed == null || (bool)showSeed.Value;
        var onlyShowInvokerTheSeed = onlyShowInvoker != null && (bool)onlyShowInvoker.Value;

        var request = new SeedGenerationRequest
        {
            Command = command,
            CosmeticsString = cosmetics,
            SettingsString = guildConfig.RaceConfig.SettingsString,
            OnlyRespondToCaller = onlyShowInvokerTheSeed,
            Seed = seedNum,
            ShowSeedInfoInResponse = showSeedInfo,
            UploadSpoiler = spoiler,
            IsRaceSeed = isRaceSeed,
        };

        Queue.Enqueue(request);

        await command.RespondAsync($"You are number {Queue.GetQueueCount()} in the queue!", ephemeral: true);
    }
    
    internal SlashCommandBuilder BuildSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Constants.GenerateRaceSeed)
            .WithDescription("Generates a seed and uploads the patch if generation succeeded.")
            .AddOption(IsSeedForRace, ApplicationCommandOptionType.Boolean, "Set true if this is for a race. If true, it will DM you the spoiler log & hide seed number.", isRequired: true)
            .AddOption(Seed, ApplicationCommandOptionType.Integer, "The seed number to use for generation.", isRequired: false)
            .AddOption(CosmeticsString, ApplicationCommandOptionType.String, "The cosmetics string to use for generation.", isRequired: false)
            .AddOption(UploadSpoiler, ApplicationCommandOptionType.Boolean, "Choose whether the spoiler is also uploaded with the patch.", isRequired: false)
            .AddOption(ShowSeedNumber, ApplicationCommandOptionType.Boolean, "Choose whether to show the seed info in the message at the end.", isRequired: false)
            .AddOption(OnlyShowYouSeed, ApplicationCommandOptionType.Boolean, "Choose whether to show the seed only to you or to everyone. Defaults to false.", isRequired: false);
    }
}