using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinishCapRandomizerSeedGeneratorBot;
using MinishCapRandomizerSeedGeneratorBot.Actions;
using MinishCapRandomizerSeedGeneratorBot.Actions.Tasks;
using MinishCapRandomizerSeedGeneratorBot.Api;
using MinishCapRandomizerSeedGeneratorBot.Api.Handlers;
using MinishCapRandomizerSeedGeneratorBot.Guild;
using MinishCapRandomizerSeedGeneratorBot.Models;
using MinishCapRandomizerSeedGeneratorBot.Threading;
using RandomizerCore.Controllers;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var services = BuildServiceProvider();
        try
        {
            Console.SetOut(TextWriter.Synchronized(Console.Out));
            Console.SetError(TextWriter.Synchronized(Console.Error));
            var connector = services.GetRequiredService<APIConnector>();
            await connector.BuildSocketClient();
            var dispatcher = services.GetRequiredService<ThreadDispatcher>();
            var thread = new Thread(dispatcher.ThreadLoop);
            thread.Start();
            var consoleHandler = services.GetRequiredService<ConsoleThread>();
            var consoleThread = new Thread(consoleHandler.Loop);
            consoleThread.Start();
            var scheduledEventDispatcher = services.GetRequiredService<ScheduledEventDispatcher>();
            var scheduledEventThread = new Thread(scheduledEventDispatcher.ThreadingLoop);
            scheduledEventThread.Start();
            thread.Join();
            scheduledEventThread.Join();
            services.GetRequiredService<SynchronizedQueue>().Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception occurred!");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            services.GetRequiredService<GuildSettings>().WriteConfigOutput();
        }
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.TryAddSingleton<DiscordSocketClient>();
        serviceCollection.TryAddSingleton<DiscordSocketConfig>();
        serviceCollection.TryAddSingleton<APIConnector>();
        serviceCollection.TryAddSingleton<SlashCommandHandler>();
        serviceCollection.TryAddSingleton<GuildEventHandler>();
        serviceCollection.TryAddSingleton<BotInitializer>();
        serviceCollection.TryAddSingleton<SeedHandler>();
        serviceCollection.TryAddSingleton<AboutHandler>();
        serviceCollection.TryAddSingleton<AsyncHandler>();
        serviceCollection.TryAddSingleton<UpdateInfoHandler>();
        serviceCollection.TryAddSingleton<ScheduleAsyncsHandler>();
        serviceCollection.TryAddSingleton<ShufflerController>();
        serviceCollection.TryAddSingleton<SynchronizedQueue>();
        serviceCollection.TryAddSingleton<SeedGenerationAction>();
        serviceCollection.TryAddSingleton<AsyncGenerationAction>();
        serviceCollection.TryAddSingleton<ThreadDispatcher>();
        serviceCollection.TryAddSingleton<AppSettings>();
        serviceCollection.TryAddSingleton<GuildSettings>();
        serviceCollection.TryAddSingleton<ConsoleThread>();
        serviceCollection.TryAddSingleton<ScheduledEventDispatcher>();
        serviceCollection.TryAddSingleton<GenerateSeedTask>();
        serviceCollection.TryAddSingleton<ScheduledEventDispatcher>();
        serviceCollection.TryAddSingleton<PresetHandler>();

        return serviceCollection.BuildServiceProvider();
    }
}