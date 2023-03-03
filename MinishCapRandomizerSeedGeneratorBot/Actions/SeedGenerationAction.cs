using System.Text;
using Discord;
using Discord.WebSocket;
using MinishCapRandomizerSeedGeneratorBot.Actions.Tasks;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Controllers;

namespace MinishCapRandomizerSeedGeneratorBot.Actions;

public class SeedGenerationAction
{
    internal GenerateSeedTask SeedTask { get; set; }
    
    public SeedGenerationAction(GenerateSeedTask seedTask)
    {
        SeedTask = seedTask;
    }
    
    public async Task Randomize(SeedGenerationRequest request)
    {
        var responseBuilder = new StringBuilder();

        var shuffler = new ShufflerController();

        var files = await SeedTask.GenerateSeed(shuffler, 1, request, responseBuilder);

        // responseBuilder.Append(shuffler.AppName).Append(' ')
        //     .Append(shuffler.VersionName)
        //     .Append(' ').Append(shuffler.RevName)
        //     .Append(' ').AppendLine("Initialized!");
        // var time = DateTime.Now;
        // responseBuilder.AppendLine("Beginning seed generation...");
        //
        // shuffler.LoadLogicFile();
        //
        // shuffler.SetRandomizationSeed(request.Seed);
        //
        // if (!string.IsNullOrEmpty(request.SettingsString))
        // {
        //     var result = shuffler.LoadSettingsFromSettingString(request.SettingsString);
        //     if (!result.WasSuccessful)
        //     {
        //         responseBuilder.AppendLine("[ERROR] Failed to load settings string! Please make sure the settings string is created on the newest release of the randomizer and uses default logic!");
        //         await ReturnErrorResponse(responseBuilder, request.Command);
        //         return;
        //     }
        //     
        //     responseBuilder.AppendLine("[INFO] Settings string loaded successfully");
        // }
        //
        // if (!string.IsNullOrEmpty(request.CosmeticsString))
        // {
        //     var result = shuffler.LoadCosmeticsFromCosmeticsString(request.CosmeticsString);
        //     if (!result.WasSuccessful)
        //     {
        //         responseBuilder.AppendLine(
        //             "[ERROR] Failed to load cosmetics string! Please make sure the cosmetics string is created on the newest release of the randomizer and uses default logic!");
        //         await ReturnErrorResponse(responseBuilder, request.Command);
        //         return;
        //     }
        //
        //     responseBuilder.AppendLine("[INFO] Cosmetics string loaded successfully");
        // }
        //
        // if (request.ShowSeedInfoInResponse)
        //     responseBuilder.Append("[INFO] Seed number: ").Append(request.Seed).AppendLine();
        //
        // shuffler.LoadLocations();
        //
        // var shufflerResult = shuffler.Randomize();
        //
        // if (!shufflerResult.WasSuccessful)
        // {
        //     responseBuilder.Append("[ERROR] Failed to randomize! Shuffler returned error: ")
        //         .AppendLine(shufflerResult.ErrorMessage ?? shufflerResult.Error?.Message);
        //     await ReturnErrorResponse(responseBuilder, request.Command);
        //     return;
        // }
        //
        // responseBuilder.AppendLine("[INFO] Seed randomized successfully");
        //
        // var patch = shuffler.CreatePatch();

        if (files.patch == null) return;
        
        var attachments = new List<FileAttachment>();

        using var patchStream = new MemoryStream(files.patch.Content);
        using var spoilerStream = new MemoryStream();
        // ReSharper disable once UseAwaitUsing
        using var writer = new StreamWriter(spoilerStream);
        
        attachments.Add(new FileAttachment(patchStream, "Patch.bps"));

        if (request.UploadSpoiler && files.spoiler != null)
        {
            // ReSharper disable MethodHasAsyncOverload
            writer.Write(files.spoiler);
            writer.Flush();
            // ReSharper enable MethodHasAsyncOverload
            spoilerStream.Position = 0;
            attachments.Add(new FileAttachment(spoilerStream, "Spoiler Log.txt", isSpoiler: true));
        }

        // responseBuilder.Append("[INFO] Settings: ").AppendLine(shuffler.GetSettingsString());
        // responseBuilder.Append("[INFO] Cosmetics: ").AppendLine(shuffler.GetCosmeticsString());
        // responseBuilder.AppendLine("...Seed generation succeeded!");
        // var totalTime = DateTime.Now - time;
        // responseBuilder.Append("Total execution time: ").Append(totalTime.Seconds).Append('.')
        //     .Append(totalTime.Milliseconds.ToString("000")).AppendLine(" seconds!");

        await request.Command.FollowupWithFilesAsync(attachments, responseBuilder.ToString(), ephemeral: request.OnlyRespondToCaller);
    }

    private async Task ReturnErrorResponse(StringBuilder responseBuilder, SocketSlashCommand command)
    {
        await command.FollowupAsync(responseBuilder.ToString(), ephemeral: true);
    }
}