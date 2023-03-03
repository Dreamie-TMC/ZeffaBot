using System.Reflection;
using Newtonsoft.Json;

namespace MinishCapRandomizerSeedGeneratorBot.Models;

public class PresetHandler
{
    internal List<Presets> Presets { get; set; }

    public PresetHandler()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("MinishCapRandomizerSeedGeneratorBot.Resources.Presets.json");
        using var reader = new StreamReader(stream);
        Presets = JsonConvert.DeserializeObject<List<Presets>>(reader.ReadToEnd()) ?? new List<Presets>();
    }
}