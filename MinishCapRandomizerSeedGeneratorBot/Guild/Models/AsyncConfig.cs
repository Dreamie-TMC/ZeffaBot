namespace MinishCapRandomizerSeedGeneratorBot.Guild.Models;

public class AsyncConfig
{
    public bool SupportsWeeklyAutomaticAsyncGeneration { get; set; }
    
    public DayOfWeek AsyncGenerationDayOfWeek { get; set; }
    
    public List<AsyncStrings> AsyncGenerationSettingAndCosmeticStrings { get; set; }
    
    public int TotalSeedsToGenerate { get; set; }
}