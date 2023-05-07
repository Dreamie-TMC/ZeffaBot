﻿using Discord.WebSocket;

namespace MinishCapRandomizerSeedGeneratorBot.Threading.Models;

public class SeedGenerationRequest
{
    public SocketSlashCommand Command { get; set; }
    public ulong Seed { get; set; }
    public string SettingsString { get; set; }
    public string CosmeticsString { get; set; }
    public bool UploadSpoiler { get; set; }
    public bool ShowSeedInfoInResponse { get; set; }
    public bool OnlyRespondToCaller { get; set; }
    public bool IsRaceSeed { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAutomatedGeneration { get; set; }
    public int AsyncSeedNumber { get; set; }
    public ulong GuildNumber { get; set; }
}