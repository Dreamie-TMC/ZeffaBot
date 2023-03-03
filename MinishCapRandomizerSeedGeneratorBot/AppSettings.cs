using Microsoft.Extensions.Configuration;
using MinishCapRandomizerSeedGeneratorBot.Models;

namespace MinishCapRandomizerSeedGeneratorBot;

public class AppSettings
{
    private readonly IConfigurationRoot _config;
    
    //This depends on a config file that contains an access token and as such is not on the repository
    public AppSettings()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public virtual string? DiscordAccessToken => _config["AccessToken"];
    public virtual string? RomPath => _config["RomPath"];
}