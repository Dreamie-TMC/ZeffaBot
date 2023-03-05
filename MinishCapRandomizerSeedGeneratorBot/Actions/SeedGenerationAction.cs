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

        if (request.IsRaceSeed)
        {
            request.ShowSeedInfoInResponse = false;
            request.UploadSpoiler = true;
            request.OnlyRespondToCaller = false;
        }

        var files = await SeedTask.GenerateSeed(shuffler, 1, request, responseBuilder);

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
            if (!request.IsRaceSeed)
                attachments.Add(new FileAttachment(spoilerStream, "Spoiler Log.txt", isSpoiler: true));
        }
        
        await request.Command.FollowupWithFilesAsync(attachments, 
            request.IsRaceSeed ? "Race seed generated! Here is the patch, happy racing!" : responseBuilder.ToString(), 
            ephemeral: request.OnlyRespondToCaller);
        
        if (request.IsRaceSeed)
        {
            try
            {
                await request.Command.User.SendFileAsync(new FileAttachment(spoilerStream,
                        $"Spoiler Log.txt", isSpoiler: true),
                    $"Here is the spoiler log for the race seed you generated!\nHere is the output log:\n{responseBuilder}");
            }
            catch
            {
                // ignored
            }
        }
    }
}