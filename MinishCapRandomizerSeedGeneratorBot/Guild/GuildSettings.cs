using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Guild.Models;
using Newtonsoft.Json;

namespace MinishCapRandomizerSeedGeneratorBot.Guild;

public class GuildSettings
{
    internal Dictionary<ulong, GuildConfig> GuildConfigs;

    public GuildSettings()
    {
        if (!File.Exists($"{AppContext.BaseDirectory}/config.json"))
        {
            GuildConfigs = new Dictionary<ulong, GuildConfig>();
            return;
        }

        try
        {
            var configData = File.ReadAllText($"{AppContext.BaseDirectory}/config.json");
            GuildConfigs = JsonConvert.DeserializeObject<Dictionary<ulong, GuildConfig>>(configData)!;
        }
        catch
        {
            GuildConfigs = new Dictionary<ulong, GuildConfig>();
        }
    }

    public void WriteConfigOutput()
    {
        var config = JsonConvert.SerializeObject(GuildConfigs);
        File.WriteAllText($"{AppContext.BaseDirectory}/config.json", config);
    }

    public void AddGuildToConfig(SocketGuild guild)
    {
        if (GuildConfigs.ContainsKey(guild.Id)) return;
        
        GuildConfigs.Add(guild.Id, new GuildConfig
        {
            Guild = new Models.Guild
            {
                GuildId = guild.Id,
            }
        });
    }

    public void AddGuildToConfig(ulong guildId)
    {
        if (GuildConfigs.ContainsKey(guildId)) return;
        
        GuildConfigs.Add(guildId, new GuildConfig
        {
            Guild = new Models.Guild
            {
                GuildId = guildId,
            },
        });
    }

    public List<GuildConfig> GetGuildsFromFilter(Func<GuildConfig, bool> filter)
    {
        return GuildConfigs.Values.ToList().Where(filter).ToList();
    }

    public GuildConfig? GetGuildFromGuildId(ulong guildId)
    {
        return !GuildConfigs.ContainsKey(guildId) ? null : GuildConfigs[guildId];
    }

    public void UpdateGuildConfig<T>(T config, ulong guildId)
    {
        if (!GuildConfigs.ContainsKey(guildId)) return;

        var guild = GuildConfigs[guildId];
        switch (config)
        {
            case AsyncConfig asyncConfig:
                guild.AsyncConfig = asyncConfig;
                break;
            case RaceConfig raceConfig:
                guild.RaceConfig = raceConfig;
                break;
        }
    }

    public void RemoveGuildFromConfig(ulong guildId)
    {
        if (!GuildConfigs.ContainsKey(guildId)) return;

        GuildConfigs.Remove(guildId);
    }
}