using System.Diagnostics.Metrics;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Random;

namespace MinishCapRandomizerSeedGeneratorBot.Threading;

public class ScheduledEventDispatcher
{
    private volatile bool _exiting = false;
    private volatile bool _guildCountUpdated = false;
    private DayOfWeek _lastPollDayOfWeek;
    
    internal GuildSettings Settings { get; set; }
    internal SynchronizedQueue Queue { get; set; }

    private const int YieldTimeMs = 60000;

    public ScheduledEventDispatcher(GuildSettings settings, SynchronizedQueue queue)
    {
        Settings = settings;
        Queue = queue;
        _lastPollDayOfWeek = DateTime.Today.DayOfWeek;
    }
    
    public void ThreadingLoop()
    {
        Thread.Sleep(10000);
        var guildsWithAsyncSupport = Settings.GetGuildsFromFilter(guild => guild.AsyncConfig != null && guild.AsyncConfig.SupportsWeeklyAutomaticAsyncGeneration);
        while (!_exiting)
        {
            if (_guildCountUpdated)
            {
                guildsWithAsyncSupport = Settings.GetGuildsFromFilter(guild => guild.AsyncConfig.SupportsWeeklyAutomaticAsyncGeneration);
                _guildCountUpdated = false;
            }

            var today = DateTime.Today.DayOfWeek;
            if (today != _lastPollDayOfWeek)
            {
                var guildsToGenerateAsyncsFor =
                    guildsWithAsyncSupport.Where(guild => guild.AsyncConfig.AsyncGenerationDayOfWeek == today);

                foreach (var guild in guildsToGenerateAsyncsFor)
                {
                    for (var i = 0; i < guild.AsyncConfig.TotalSeedsToGenerate; ++i)
                    {            
                        var seed = new SquaresRandomNumberGenerator().Next();
                        var strings =
                            guild.AsyncConfig.AsyncGenerationSettingAndCosmeticStrings.FirstOrDefault(x => x.SeedNumber == i + 1);
                        
                        Queue.Enqueue(new SeedGenerationRequest
                        {
                            AsyncSeedNumber = i + 1,
                            Seed = seed,
                            SettingsString = strings?.SettingsString ?? "",
                            CosmeticsString = strings?.CosmeticsString ?? "",
                            OnlyRespondToCaller = false,
                            ShowSeedInfoInResponse = false,
                            UploadSpoiler = true,
                            IsAsync = true,
                            GuildNumber = guild.Guild.GuildId,
                            IsAutomatedGeneration = true,
                            Command = null!,
                        });
                    }
                }
            }

            _lastPollDayOfWeek = today;
            Thread.Sleep(YieldTimeMs);
        }
    }

    public void SetExiting()
    {
        _exiting = true;
    }

    public void SetGuildCountUpdated()
    {
        _guildCountUpdated = true;
    }
}