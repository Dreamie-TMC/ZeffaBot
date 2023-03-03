using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Threading.Models;

public class SeedGenerationRequest
{
    public SocketSlashCommand Command { get; set; }
    public int Seed { get; set; }
    public string SettingsString { get; set; }
    public string CosmeticsString { get; set; }
    public bool UploadSpoiler { get; set; }
    public bool ShowSeedInfoInResponse { get; set; }
    public bool OnlyRespondToCaller { get; set; }
    public bool IsAsync { get; set; }
    public int AsyncSeedNumber { get; set; }
    public ulong GuildNumber { get; set; }
    public ulong ChannelNumber { get; set; }
}