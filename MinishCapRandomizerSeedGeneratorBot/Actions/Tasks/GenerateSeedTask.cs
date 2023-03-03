using System.Text;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Controllers;
using RandomizerCore.Utilities.Models;

namespace MinishCapRandomizerSeedGeneratorBot.Actions.Tasks;

public class GenerateSeedTask
{
    public async Task<(PatchFile? patch, string? spoiler)> GenerateSeed(ShufflerController shuffler, int retryAttempts, SeedGenerationRequest request, StringBuilder responseBuilder, bool doNotRespondWithErrors = false)
    {
        responseBuilder.Append(shuffler.AppName).Append(' ')
            .Append(shuffler.VersionName)
            .Append(' ').Append(shuffler.RevName)
            .Append(' ').AppendLine("Initialized!");
        
        var time = DateTime.Now;
        responseBuilder.AppendLine("Beginning seed generation...");

        shuffler.LoadLogicFile();
        
        shuffler.SetRandomizationSeed(request.Seed);
        
        if (!string.IsNullOrEmpty(request.SettingsString))
        {
            var result = shuffler.LoadSettingsFromSettingString(request.SettingsString);
            if (!result.WasSuccessful)
            {
                responseBuilder.AppendLine("[ERROR] Failed to load settings string! Please make sure the settings string is created on the newest release of the randomizer and uses default logic!");
                await ReturnErrorResponse(responseBuilder, request.Command, doNotRespondWithErrors);
                return (null, null);
            }
            
            responseBuilder.AppendLine("[INFO] Settings string loaded successfully");
        }

        if (!string.IsNullOrEmpty(request.CosmeticsString))
        {
            var result = shuffler.LoadCosmeticsFromCosmeticsString(request.CosmeticsString);
            if (!result.WasSuccessful)
            {
                responseBuilder.AppendLine(
                    "[ERROR] Failed to load cosmetics string! Please make sure the cosmetics string is created on the newest release of the randomizer and uses default logic!");
                await ReturnErrorResponse(responseBuilder, request.Command, doNotRespondWithErrors);
                return (null, null);
            }

            responseBuilder.AppendLine("[INFO] Cosmetics string loaded successfully");
        }

        if (request.ShowSeedInfoInResponse)
            responseBuilder.Append("[INFO] Seed number: ").Append(request.Seed).AppendLine();
        
        shuffler.LoadLocations();

        var shufflerResult = shuffler.Randomize(retryAttempts);

        if (!shufflerResult.WasSuccessful)
        {
            responseBuilder.Append("[ERROR] Failed to randomize! Shuffler returned error: ")
                .AppendLine(shufflerResult.ErrorMessage ?? shufflerResult.Error?.Message);
            await ReturnErrorResponse(responseBuilder, request.Command, doNotRespondWithErrors);
            return (null, null);
        }

        responseBuilder.AppendLine("[INFO] Seed randomized successfully");

        var patch = shuffler.CreatePatch();
        string? spoiler = null;

        responseBuilder.AppendLine("[INFO] Patch created successfully");

        if (request.UploadSpoiler)
        {
            spoiler = shuffler.CreateSpoiler();
            responseBuilder.AppendLine("[INFO] Spoiler created successfully");
        }

        responseBuilder.Append("[INFO] Settings: ").AppendLine(shuffler.GetSettingsString());
        responseBuilder.Append("[INFO] Cosmetics: ").AppendLine(shuffler.GetCosmeticsString());
        responseBuilder.AppendLine("...Seed generation succeeded!");
        
        var totalTime = DateTime.Now - time;
        responseBuilder.Append("Total execution time: ").Append(totalTime.Seconds).Append('.')
            .Append(totalTime.Milliseconds.ToString("000")).AppendLine(" seconds!");

        return (patch, spoiler);
    }

    private async Task ReturnErrorResponse(StringBuilder responseBuilder, SocketSlashCommand command, bool doNotRespondWithErrors)
    {
        if (doNotRespondWithErrors) return;
        
        await command.FollowupAsync(responseBuilder.ToString(), ephemeral: true);
    }
}