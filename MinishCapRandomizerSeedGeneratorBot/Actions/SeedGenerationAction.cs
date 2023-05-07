using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using Discord;
using MinishCapRandomizerSeedGeneratorBot.Actions.Tasks;
using MinishCapRandomizerSeedGeneratorBot.Threading.Models;
using RandomizerCore.Controllers;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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

        using var hashStream = new MemoryStream();
        InitializeHash(shuffler).Save(hashStream, ImageFormat.Png);
        
        attachments.Add(new FileAttachment(hashStream, "Seed Hash.png"));

        await request.Command.FollowupWithFilesAsync(attachments,
            request.IsRaceSeed ? "Race seed generated! Here is the patch, happy racing!" : responseBuilder.ToString(),
            ephemeral: request.OnlyRespondToCaller);
        
        if (request.IsRaceSeed)
        {
            try
            {
                await request.Command.User.SendFilesAsync(new[]
                    {
                        new FileAttachment(spoilerStream,
                            "Spoiler Log.txt", isSpoiler: true),
                    },
                    $"Here is the spoiler log for the race seed you generated!\nHere is the output log:\n{responseBuilder}");
            }
            catch
            {
                // ignored
            }
        }
    }
    
    private Bitmap InitializeHash(ShufflerController shuffler)
    {
        const byte hashMask = 0b111111;
        
        var badBgColor = Color.FromArgb(0x30, 0xa0, 0xac).ToArgb();
        var otherBadBgColor = Color.FromArgb(0x30, 0xa0, 0x78).ToArgb();
        var newBgColor = Color.FromArgb(0x08, 0x19, 0xad);
        var eventDefines = shuffler.GetEventWrites().Split('\n');
        var customRng = uint.Parse(eventDefines.First(line => line.Contains("customRNG")).Split(' ')[2][2..], 
            NumberStyles.HexNumber);
        var seed = uint.Parse(eventDefines.First(line => line.Contains("seedHashed")).Split(' ')[2][2..],
            NumberStyles.HexNumber);
        var settings = uint.Parse(eventDefines.First(line => line.Contains("settingHash")).Split(' ')[2][2..],
            NumberStyles.HexNumber);

        using var stream = Assembly.GetAssembly(typeof(ShufflerController))?.GetManifestResourceStream("RandomizerCore.Resources.hashicons.png");
        var hashImageList = new Bitmap(stream);

        var hashImage = new Bitmap(384, 48);
        
        for (var imageNum = 0; imageNum < 8; ++imageNum)
        {

            var imageIndex = imageNum switch
            {
                0 => (seed >> 24) & hashMask,
                1 => (seed >> 16) & hashMask,
                2 => (seed >> 8) & hashMask,
                3 => seed & hashMask,
                4 => (customRng >> 8) & hashMask,
                5 => 64U,
                6 => (settings >> 8) & hashMask,
                7 => (settings >> 16) & hashMask
            };

            var k = 16 * (int)imageIndex;
            var l = 16 * imageNum;
            for (var i = 0; i < 16; ++i)
            {
                var i3 = i * 3;
                for (var j = 0; j < 16; ++j)
                {
                    var color = hashImageList.GetPixel(j, i + k);
                    var argb = color.ToArgb();
                    if (argb == badBgColor || argb == otherBadBgColor)
                        color = newBgColor;
                    AddColorToImage3X(hashImage, color, j + l, i3);
                    AddColorToImage3X(hashImage, color, j + l, i3 + 1);
                    AddColorToImage3X(hashImage, color, j + l, i3 + 2);
                }
            }
        }
        
        return hashImage;
    }

    private static void AddColorToImage3X(Bitmap baseImage, System.Drawing.Color targetColor, int x, int y)
    {
        x *= 3;
#pragma warning disable CA1416
        baseImage.SetPixel(x, y, targetColor);
        baseImage.SetPixel(x + 1, y, targetColor);
        baseImage.SetPixel(x + 2, y, targetColor);
#pragma warning restore CA1416
    }
}