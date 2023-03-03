using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;

namespace MinishCapRandomizerSeedGeneratorBot.Api.Handlers;

public class AsyncHandler
{
    internal static readonly string Count = "num-seeds";
    internal static readonly string SettingsString = "settings-string";
    internal static readonly string CosmeticsString = "cosmetics-string";
    
    internal DiscordSocketClient SocketClient { get; set; }
    internal SynchronizedQueue Queue { get; set; }

    public AsyncHandler(DiscordSocketClient client, SynchronizedQueue queue)
    {
        SocketClient = client;
        Queue = queue;
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
            var settingsString = parameters.FirstOrDefault(param => param.Name == $"{SettingsString}-{i + 1}");
            var cosmeticsString = parameters.FirstOrDefault(param => param.Name == $"{CosmeticsString}-{i + 1}");

            var seed = new Random().Next();
            var settings = settingsString == null ? "" : (string)settingsString.Value;
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
                ChannelNumber = command.ChannelId!.Value,
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
            .AddOption(Count, ApplicationCommandOptionType.Integer,
                "The number of async seeds to generate. Must be a number from 1 to 5.", isRequired: true)
            .AddOption($"{SettingsString}-1", ApplicationCommandOptionType.String,
                "The settings string to use for seed 1. Leave blank for default.", isRequired: false)
            .AddOption($"{CosmeticsString}-1", ApplicationCommandOptionType.String,
                "The cosmetics string to use for seed 1. Leave blank for default.", isRequired: false)
            .AddOption($"{SettingsString}-2", ApplicationCommandOptionType.String,
                "The settings string to use for seed 2. Leave blank for default.", isRequired: false)
            .AddOption($"{CosmeticsString}-2", ApplicationCommandOptionType.String,
                "The cosmetics string to use for seed 2. Leave blank for default.", isRequired: false)
            .AddOption($"{SettingsString}-3", ApplicationCommandOptionType.String,
                "The settings string to use for seed 3. Leave blank for default.", isRequired: false)
            .AddOption($"{CosmeticsString}-3", ApplicationCommandOptionType.String,
                "The cosmetics string to use for seed 3. Leave blank for default.", isRequired: false)
            .AddOption($"{SettingsString}-4", ApplicationCommandOptionType.String,
                "The settings string to use for seed 4. Leave blank for default.", isRequired: false)
            .AddOption($"{CosmeticsString}-4", ApplicationCommandOptionType.String,
                "The cosmetics string to use for seed 4. Leave blank for default.", isRequired: false)
            .AddOption($"{SettingsString}-5", ApplicationCommandOptionType.String,
                "The settings string to use for seed 5. Leave blank for default.", isRequired: false)
            .AddOption($"{CosmeticsString}-5", ApplicationCommandOptionType.String,
                "The cosmetics string to use for seed 5. Leave blank for default.", isRequired: false);
    }
}